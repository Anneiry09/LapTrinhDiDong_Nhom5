import os
import joblib
import pandas as pd
import random
from google import genai
from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import List, Optional

# Gemini API — đọc key từ .env (GEMINI_API_KEY)
_gemini_key = os.getenv("GEMINI_API_KEY", "")
_gemini_client = None
if _gemini_key:
    try:
        _gemini_client = genai.Client(api_key=_gemini_key)
        print("[OK] Gemini API da san sang (model: gemini-flash-latest)")
    except Exception as e:
        print(f"[WARN] Khong the khoi tao Gemini: {e}")
else:
    print("[INFO] GEMINI_API_KEY chua cau hinh -- dung Rule-based fallback cho huong dan nau an")

ai_router = APIRouter(prefix="/api/ai", tags=["AI Engine"])

# ==========================================
# 1. LOAD MÔ HÌNH (HYBRID AI ML)
# ==========================================
_dir = os.path.dirname(os.path.abspath(__file__))
exercise_scorer = None
try:
    print("[AI] Dang tai AI Model: exercise_scorer.pkl...")
    exercise_scorer = joblib.load(os.path.join(_dir, "exercise_scorer.pkl"))
    print("[OK] Load mo hinh exercise_scorer thanh cong!")
except Exception as e:
    print(f"[WARN] Loi load model: {str(e)}")

# ==========================================
# 2. MODELS & SCHEMAS
# ==========================================
class UserContext(BaseModel):
    age: float
    bmi: float
    isMale: int
    targetGoal: int     # 1: Lose, 2: Maintain, 3: Gain
    fitnessLevel: int   # 1: Beg, 2: Int, 3: Adv

class AvailableExercise(BaseModel):
    id: int
    name: str
    baseCaloriesBurned: float
    type: int           # 1: Bodyweight, 2: Cardio, 3: Dumbbell, 4: Machine, 5: Yoga
    muscleGroup: str

class CustomWorkoutRequest(BaseModel):
    user: UserContext
    maxDurationMins: int
    workoutType: int
    hasWarmup: bool
    hasCooldown: bool
    targetMuscleGroups: Optional[List[str]] = None
    availableExercises: List[AvailableExercise]

def matches_target_groups(exercise_muscle: str, exercise_name: str, target_groups: List[str]) -> bool:
    if not target_groups:
        return True
    
    mapping = {
        "ngực": ["ngực", "nguc", "chest", "push up", "pushup", "bench press", "chest press", "squeeze press", "fly", "pec", "ép ngực", "đẩy ngực"],
        "lưng": ["lưng", "lung", "back", "pull up", "pullup", "row", "chin up", "lat", "pulldown", "deadlift", "erector", "lats", "chèo thuyền", "kéo xà", "kéo lưng"],
        "vai": ["vai", "shoulder", "delt", "shrug", "lateral raise", "front raise", "shoulder raise", "delt raise", "cầu vai", "cau vai", "traps", "dang vai", "ép vai", "đẩy vai"],
        "tay": ["tay", "arm", "bicep", "tricep", "bicep curl", "biceps curl", "hammer curl", "wrist curl", "tricep extension", "triceps extension", "overhead extension", "french press", "tate press", "kickback tay", "tricep kickback", "dip", "cẳng tay", "cang tay", "forearm", "triceps", "biceps", "cuốn bắp tay", "cuốn tay", "duỗi tay", "nhún vai"],
        "chân": ["chân", "chan", "leg", "squat", "lunge", "calf", "thigh", "leg press", "hack squat", "đùi", "dui", "mông", "mong", "glute", "quad", "hamstring", "bắp chân", "bap chan", "nhón chân", "nhon chan", "hip thrust", "glute kickback", "cable kickback", "leg curl", "leg extension", "bắp đùi", "bap dui"],
        "bụng": ["bụng", "bung", "core", "abs", "plank", "crunch", "twist", "abdominal", "sườn", "suon", "oblique", "dead bug", "hollow body", "v-up", "sit up", "situp", "gập bụng", "xoay eo"],
        "mông": ["mông", "mong", "glute", "hip thrust", "glute kickback", "cable kickback", "squat", "lunge", "mông kickback", "kickback mông"],
        "đùi": ["đùi", "dui", "quad", "hamstring", "squat", "lunge", "leg press", "hack squat", "leg curl", "leg extension", "bắp đùi", "bap dui"],
        "bắp tay": ["tay", "arm", "bicep", "tricep", "bicep curl", "biceps curl", "hammer curl", "wrist curl", "tricep extension", "triceps extension", "dip", "triceps", "biceps", "cuốn bắp tay", "cuốn tay"]
    }
    
    exercise_muscle = exercise_muscle.lower()
    exercise_name = exercise_name.lower()
    
    for tg in target_groups:
        tg_clean = tg.lower().strip()
        synonyms = mapping.get(tg_clean, [tg_clean])
        if any(syn in exercise_muscle or syn in exercise_name for syn in synonyms):
            return True
            
    return False

class AvailableRecipe(BaseModel):
    id: int
    name: str
    calories: float
    protein: float
    carbs: float
    fat: float
    price: float
    mealSuitability: int = 6  # bitmask: 1=Breakfast, 2=Lunch, 4=Dinner, 8=Snack
    category: int = 1

class MealPlanRequest(BaseModel):
    tdee: float
    targetGoal: int
    dailyBudget: float
    availableRecipes: List[AvailableRecipe]
    recentRecipeIds: List[int] = []  # IDs của recipes đã dùng 2-3 ngày gần đây (Repetition Penalty)

# ==========================================
# 3. ENDPOINTS
# ==========================================

@ai_router.post("/generate-custom-session")
def generate_custom_session(req: CustomWorkoutRequest):
    """
    Hybrid AI: 
    - ML để chấm điểm bài tập (Suitability Score).
    - Thuật toán Heuristic (Knapsack biến thể) để sắp xếp thời gian.
    """
    if exercise_scorer is None:
        raise HTTPException(status_code=503, detail="AI Scorer Model chưa sẵn sàng.")

    try:
        # 1. Chuẩn bị biến thời gian (Mỗi bài tập giả định tốn 8 phút)
        MINS_PER_EXERCISE = 8.0 
        core_duration = req.maxDurationMins
        warmup_duration = 0
        cooldown_duration = 0

        if req.hasWarmup:
            warmup_duration = 10
            core_duration -= 10
        if req.hasCooldown:
            cooldown_duration = 10
            core_duration -= 10

        if core_duration <= 0:
            raise HTTPException(status_code=400, detail="Thời gian quá ngắn để tập chính.")

        max_core_exercises = int(core_duration // MINS_PER_EXERCISE)

        # 2. Lọc và phân loại bài tập
        warmup_pool = [e for e in req.availableExercises if e.type == 2] # Giả sử Cardio nhẹ làm Warmup
        if not warmup_pool:
            warmup_pool = [e for e in req.availableExercises if "cardio" in e.name.lower() or "chạy" in e.name.lower() or "nhảy" in e.name.lower()]
        if not warmup_pool:
            warmup_pool = req.availableExercises

        cooldown_pool = [e for e in req.availableExercises if e.type == 5] # Giả sử Yoga làm Cooldown
        if not cooldown_pool:
            cooldown_pool = [e for e in req.availableExercises if "giãn" in e.name.lower() or "stretch" in e.name.lower() or "yoga" in e.name.lower()]
        if not cooldown_pool:
            cooldown_pool = req.availableExercises
        
        # Bài tập chính: ưu tiên thể loại user chọn
        if req.workoutType == 4:
            # Gym / Tạ bao gồm cả Dumbbell (3) và Machine (4) để pool bài tập phong phú (11 bài)
            core_pool = [e for e in req.availableExercises if e.type in (3, 4)]
        else:
            core_pool = [e for e in req.availableExercises if e.type == req.workoutType]
            
        if not core_pool:
            if req.workoutType in (1, 3, 4):
                core_pool = [e for e in req.availableExercises if e.type in (1, 3, 4)]
            if not core_pool:
                core_pool = req.availableExercises

        # Lọc nâng cao theo danh sách nhóm cơ mục tiêu (Target Muscle Groups)
        if req.targetMuscleGroups:
            filtered_pool = [
                e for e in core_pool 
                if matches_target_groups(e.muscleGroup, e.name, req.targetMuscleGroups)
            ]
            if filtered_pool:
                core_pool = filtered_pool
            else:
                # Nếu không tìm thấy trong core_pool nhưng workoutType thuộc nhóm Sức mạnh (1, 3, 4),
                # thử tìm trong toàn bộ bể bài tập Sức mạnh của availableExercises trước khi hủy bộ lọc
                if req.workoutType in (1, 3, 4):
                    strength_pool = [e for e in req.availableExercises if e.type in (1, 3, 4)]
                    alt_filtered = [
                        e for e in strength_pool
                        if matches_target_groups(e.muscleGroup, e.name, req.targetMuscleGroups)
                    ]
                    if alt_filtered:
                        core_pool = alt_filtered

        # 3. Dùng ML (Random Forest) chấm điểm (Scoring) cho Core Pool
        features = []
        for ex in core_pool:
            features.append({
                'User_Age': req.user.age,
                'User_BMI': req.user.bmi,
                'Is_Male': req.user.isMale,
                'Target_Goal': req.user.targetGoal,
                'Fitness_Level': req.user.fitnessLevel,
                'Exercise_Type': ex.type,
                'Exercise_BaseCalories': ex.baseCaloriesBurned
            })
        
        df_features = pd.DataFrame(features)
        predicted_scores = exercise_scorer.predict(df_features)

        # Gắn điểm vào bài tập và sắp xếp giảm dần
        scored_exercises = []
        for i, ex in enumerate(core_pool):
            scored_exercises.append({
                "exercise": ex,
                "score": predicted_scores[i]
            })
        
        scored_exercises.sort(key=lambda x: x["score"], reverse=True)

        # 4. Tối ưu hóa lựa chọn (Knapsack variant - chọn Top N bài điểm cao nhất)
        selected_core = scored_exercises[:max_core_exercises]

        # 5. Đóng gói kết quả
        result_plan = []
        
        if req.hasWarmup and warmup_pool:
            warmup_exercise = random.choice(warmup_pool)
            result_plan.append({
                "phase": "Warm-up",
                "exerciseId": warmup_exercise.id,
                "name": warmup_exercise.name,
                "durationMins": warmup_duration,
                "caloriesBurned": warmup_exercise.baseCaloriesBurned * (warmup_duration/10.0)
            })

        for item in selected_core:
            ex = item["exercise"]
            
            # Determine sets, reps, restTimeSeconds based on user's fitnessLevel (1: Beg, 2: Int, 3: Adv)
            level = req.user.fitnessLevel
            if level == 1:
                sets_val = 2
                reps_val = 10
                rest_val = 90
            elif level == 3:
                sets_val = 4
                reps_val = 15
                rest_val = 45
            else:
                sets_val = 3
                reps_val = 12
                rest_val = 60

            result_plan.append({
                "phase": "Core Workout",
                "exerciseId": ex.id,
                "name": ex.name,
                "durationMins": MINS_PER_EXERCISE,
                "ml_suitability_score": round(item["score"], 2),
                "caloriesBurned": ex.baseCaloriesBurned,
                "sets": sets_val,
                "reps": reps_val,
                "restTimeSeconds": rest_val
            })

        if req.hasCooldown and cooldown_pool:
            cooldown_exercise = random.choice(cooldown_pool)
            result_plan.append({
                "phase": "Cool-down",
                "exerciseId": cooldown_exercise.id,
                "name": cooldown_exercise.name,
                "durationMins": cooldown_duration,
                "caloriesBurned": cooldown_exercise.baseCaloriesBurned * (cooldown_duration/10.0)
            })

        total_calo = sum([p["caloriesBurned"] for p in result_plan])

        return {
            "status": "success",
            "totalDurationMins": warmup_duration + (len(selected_core)*MINS_PER_EXERCISE) + cooldown_duration,
            "totalEstimatedCalories": round(total_calo, 2),
            "workoutPlan": result_plan
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@ai_router.post("/generate-meal-plan")
def generate_meal_plan(req: MealPlanRequest):
    """
    Thuật toán Sinh Thực Đơn v5 — Smart Recipe Selection.
    4 cải tiến so với v4:
      1. Repetition Penalty  — phạt món đã ăn 2-3 ngày gần đây
      2. Macro-Weighting     — ưu tiên macro phù hợp mục tiêu (Protein cho Gain, Low-fat cho Lose)
      3. Dynamic Multiplier  — mở rộng serving cap theo TDEE (lên 2.0x-2.5x cho TDEE > 2500)
      4. Diversity Bonus     — thưởng điểm cho recipe khác category trong cùng ngày
    """
    try:
        warnings = []

        # 1. Mục tiêu lượng calo theo goal
        target_calories = req.tdee
        if req.targetGoal == 1:  # Lose
            target_calories *= 0.85
        elif req.targetGoal == 3:  # Gain
            target_calories *= 1.10

        daily_budget = req.dailyBudget
        budget_enabled = daily_budget > 0

        # 2. Phân pool recipe theo MealSuitability bitmask
        meal_pools = {
            "Breakfast": [r for r in req.availableRecipes if r.mealSuitability & 1],
            "Lunch":     [r for r in req.availableRecipes if r.mealSuitability & 2],
            "Dinner":    [r for r in req.availableRecipes if r.mealSuitability & 4],
            "Snack":     [r for r in req.availableRecipes if r.mealSuitability & 8],
        }

        # 3. Phân bổ calo và ngân sách theo bữa
        meal_ratios = {"Breakfast": 0.25, "Lunch": 0.35, "Dinner": 0.30, "Snack": 0.10}

        # ── CẮT 2: Trọng số Macro theo TargetGoal ──
        # Mỗi goal có profile trọng số khác nhau
        # w_cal: trọng số độ lệch calo (thấp hơn = calo ít quan trọng hơn)
        # w_pro: thưởng protein cao (càng cao càng ưu tiên protein)
        # w_fat_penalty: phạt fat cao (dương = phạt nhiều fat)
        if req.targetGoal == 3:  # Gain Muscle
            w_cal, w_pro, w_fat_penalty = 0.4, 0.45, 0.15
        elif req.targetGoal == 1:  # Lose Weight
            w_cal, w_pro, w_fat_penalty = 0.35, 0.35, 0.30
        else:  # Maintain
            w_cal, w_pro, w_fat_penalty = 0.55, 0.25, 0.20

        # ── CẮT 1: Set lặp từ C# ──
        recent_ids = set(req.recentRecipeIds) if req.recentRecipeIds else set()

        # ── CẮT 3: Dynamic Serving Multiplier ──
        # Scale cap dựa trên TDEE thực tế
        if target_calories > 3000:
            multiplier_cap = 2.5
        elif target_calories > 2500:
            multiplier_cap = 2.0
        else:
            multiplier_cap = 1.5

        result_plan = []
        total_price = 0
        total_calo_generated = 0
        used_recipe_ids = set()
        used_categories = set()  # CẮT 4: track category đã dùng trong ngày

        def score_recipe(recipe, target_cal, meal_type):
            """
            Hàm tính điểm đa nhân tố.
            Điểm THẤP = TỐT HƠN (giống loss function).
            """
            # Normalize: chia cho target để các thành phần có scale tương đương
            if target_cal <= 0:
                target_cal = 500

            # Thành phần 1: Độ lệch calo (0-1 scale)
            calo_diff = abs(recipe.calories - target_cal) / target_cal
            calo_diff = min(calo_diff, 2.0)  # cap tránh outlier

            # Thành phần 2: Protein score (cao = tốt → đảo thành penalty)
            # Protein ratio = g protein / tổng calo. Lý tưởng Gain = 0.35, Lose = 0.30, Maintain = 0.25
            ideal_protein_ratio = {1: 0.30, 2: 0.25, 3: 0.35}.get(req.targetGoal, 0.25)
            if recipe.calories > 0:
                actual_protein_ratio = (recipe.protein * 4) / recipe.calories  # protein kcal / total kcal
            else:
                actual_protein_ratio = 0
            protein_penalty = max(0, ideal_protein_ratio - actual_protein_ratio)  # phạt nếu thiếu protein

            # Thành phần 3: Fat penalty (cao fat = xấu cho Lose/Gain)
            if recipe.calories > 0:
                fat_ratio = (recipe.fat * 9) / recipe.calories  # fat kcal / total kcal
            else:
                fat_ratio = 0
            # Phạt nếu fat > 35% calo
            fat_penalty = max(0, fat_ratio - 0.35)

            # Tổng hợp điểm
            score = (w_cal * calo_diff) + (w_pro * protein_penalty) + (w_fat_penalty * fat_penalty)

            # ── CẮT 1: Repetition Penalty ──
            if recipe.id in recent_ids:
                score *= 1.8  # Phạt 80% nếu đã ăn gần đây

            # ── CẮT 4: Diversity Bonus ──
            if used_categories and recipe.category not in used_categories:
                score *= 0.75  # Thưởng 25% nếu category mới trong ngày

            return score

        def pick_recipe(pool, target_cal, budget_limit, exclude_ids, meal_type):
            """Chọn 1 recipe từ pool — scoring đa nhân tố."""
            candidates = [r for r in pool if r.id not in exclude_ids and r.calories > 0]
            if not candidates:
                return None

            # Lọc theo budget nếu enabled
            if budget_enabled and budget_limit > 0:
                affordable = [r for r in candidates if r.price <= budget_limit * 1.3]
                if affordable:
                    candidates = affordable

            # Score tất cả candidates
            scored = [(r, score_recipe(r, target_cal, meal_type)) for r in candidates]
            scored.sort(key=lambda x: x[1])  # Thấp = tốt

            # Lấy top 5, random để tránh deterministic
            top_n = scored[:min(5, len(scored))]
            # Weighted random: món điểm thấp hơn có xác suất cao hơn
            if len(top_n) > 1:
                scores_inv = [1.0 / (s + 0.01) for _, s in top_n]
                total_w = sum(scores_inv)
                weights = [w / total_w for w in scores_inv]
                chosen = random.choices([r for r, _ in top_n], weights=weights, k=1)[0]
            else:
                chosen = top_n[0][0]
            return chosen

        # 4. Sinh thực đơn — mỗi bữa chọn 1 recipe
        for meal_type, pool in meal_pools.items():
            meal_calo_target = target_calories * meal_ratios[meal_type]
            meal_budget = daily_budget * meal_ratios[meal_type] if budget_enabled else float('inf')

            recipe = pick_recipe(pool, meal_calo_target, meal_budget, used_recipe_ids, meal_type)
            if recipe is None:
                continue

            # ── CẮT 3: Dynamic Multiplier ──
            if recipe.calories > 0:
                multiplier = meal_calo_target / recipe.calories
                multiplier = max(0.5, min(multiplier_cap, round(multiplier, 2)))
            else:
                multiplier = 1.0

            actual_cal = round(recipe.calories * multiplier, 1)
            actual_protein = round(recipe.protein * multiplier, 1)
            actual_carbs = round(recipe.carbs * multiplier, 1)
            actual_fat = round(recipe.fat * multiplier, 1)
            actual_price = round(recipe.price * multiplier, 0)

            # Budget check
            if budget_enabled and total_price + actual_price > daily_budget and actual_price > 0:
                remaining = daily_budget - total_price
                if remaining > 0 and recipe.price > 0:
                    multiplier = max(0.5, remaining / recipe.price)
                    actual_cal = round(recipe.calories * multiplier, 1)
                    actual_protein = round(recipe.protein * multiplier, 1)
                    actual_carbs = round(recipe.carbs * multiplier, 1)
                    actual_fat = round(recipe.fat * multiplier, 1)
                    actual_price = round(recipe.price * multiplier, 0)
                else:
                    continue

            result_plan.append({
                "mealType": meal_type,
                "recipeId": recipe.id,
                "recipeName": recipe.name,
                "calories": actual_cal,
                "protein": actual_protein,
                "carbs": actual_carbs,
                "fat": actual_fat,
                "price": actual_price,
                "servingMultiplier": multiplier
            })

            used_recipe_ids.add(recipe.id)
            used_categories.add(recipe.category)  # CẮT 4: track
            total_price += actual_price
            total_calo_generated += actual_cal

        # 5. Cảnh báo nếu calo < 70% target
        if target_calories > 0 and total_calo_generated < target_calories * 0.7:
            warnings.append(
                f"⚠️ Thực đơn chỉ đạt {total_calo_generated:.0f}/{target_calories:.0f} kcal "
                f"({total_calo_generated/target_calories*100:.0f}%). "
                f"Hãy cân nhắc tăng ngân sách để đảm bảo dinh dưỡng."
            )

        # 6. Tính % budget đã dùng
        budget_used_percent = round(total_price / daily_budget * 100, 1) if budget_enabled and daily_budget > 0 else 0

        return {
            "status": "success",
            "targetCalories": target_calories,
            "generatedCalories": round(total_calo_generated, 1),
            "totalPrice": total_price,
            "budgetLimit": daily_budget,
            "budgetUsedPercent": budget_used_percent,
            "warnings": warnings,
            "meals": result_plan
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

# ==========================================
# GENERATE COOKING INSTRUCTIONS (Gemini + Fallback)
# ==========================================

class IngredientInput(BaseModel):
    name: str
    quantityGrams: float = 0
    role: int = 1  # 1=Chính, 2=Phụ, 3=Gia vị

class GenerateInstructionsRequest(BaseModel):
    recipeName: str
    ingredients: List[IngredientInput]

def _fallback_instructions(recipe_name: str, ingredients: List[IngredientInput]) -> List[str]:
    """Rule-based fallback — sinh hướng dẫn nấu ăn offline."""
    if not ingredients:
        return [
            f"Bước 1: Chuẩn bị các nguyên liệu cơ bản cần thiết cho món {recipe_name} (rau củ, gia vị và phần đạm chính phù hợp).",
            f"Bước 2: Rửa sạch và sơ chế cắt thái nguyên liệu thành miếng vừa ăn. Ướp phần đạm chính với chút muối, tiêu và hành tỏi băm nhỏ trong 10-15 phút.",
            f"Bước 3: Đun nóng chảo dầu, phi thơm tỏi băm rồi cho nguyên liệu chính vào xào chín săn hoặc nấu chín tới tùy theo phương pháp nấu phù hợp.",
            f"Bước 4: Thêm rau củ phụ và gia vị nêm nếm cho vừa khẩu vị, đảo đều trên lửa lớn thêm 2-3 phút cho chín chín tới rồi tắt bếp.",
            f"Bước 5: Trình bày món ăn ra đĩa và thưởng thức {recipe_name} ngay khi còn nóng cùng với cơm hoặc salad kèm theo."
        ]

    main_items = [i for i in ingredients if i.role == 1]
    side_items = [i for i in ingredients if i.role == 2]
    spice_items = [i for i in ingredients if i.role == 3]

    # Nếu không phân role → gán heuristic: 2 đầu = chính, còn lại = phụ
    if not main_items and not side_items:
        main_items = ingredients[:2]
        side_items = ingredients[2:]

    steps = []
    step = 1

    # Bước 1: Sơ chế
    all_names = [i.name for i in ingredients]
    steps.append(f"Bước {step}: Chuẩn bị nguyên liệu — rửa sạch và sơ chế {', '.join(all_names)}.")
    step += 1

    # Bước 2: Sơ chế nguyên liệu chính
    if main_items:
        main_names = [f"{i.name} ({i.quantityGrams:.0f}g)" for i in main_items]
        steps.append(f"Bước {step}: Cắt/thái {', '.join(main_names)} thành miếng vừa ăn. Ướp gia vị 10 phút.")
        step += 1

    # Bước 3: Nấu chính
    name_lower = recipe_name.lower()
    if any(kw in name_lower for kw in ["chiên", "rán", "áp chảo"]):
        cook_method = "Bắc chảo, đun nóng dầu ăn. Cho nguyên liệu chính vào chiên vàng đều 2 mặt."
    elif any(kw in name_lower for kw in ["luộc", "hấp"]):
        cook_method = "Đun nước sôi, cho nguyên liệu chính vào luộc/hấp chín tới."
    elif any(kw in name_lower for kw in ["kho", "rim"]):
        cook_method = "Cho nguyên liệu chính vào nồi, thêm nước mắm và gia vị. Kho lửa nhỏ đến khi thấm đều."
    elif any(kw in name_lower for kw in ["xào"]):
        cook_method = "Bắc chảo nóng, phi tỏi thơm rồi cho nguyên liệu chính vào xào nhanh tay lửa lớn."
    elif any(kw in name_lower for kw in ["nướng"]):
        cook_method = "Làm nóng lò 200°C. Xếp nguyên liệu lên khay, nướng 15-20 phút đến khi chín vàng."
    elif any(kw in name_lower for kw in ["salad", "trộn"]):
        cook_method = "Trộn đều tất cả nguyên liệu trong tô lớn với nước sốt."
    elif any(kw in name_lower for kw in ["phở", "bún", "mì"]):
        cook_method = "Nấu nước dùng, luộc sợi phở/bún/mì riêng. Xếp vào tô và chan nước dùng nóng."
    elif any(kw in name_lower for kw in ["cháo"]):
        cook_method = "Nấu gạo với nước theo tỉ lệ 1:8, khuấy đều đến khi nhuyễn mịn."
    elif any(kw in name_lower for kw in ["sinh tố", "smoothie"]):
        cook_method = "Cho tất cả nguyên liệu vào máy xay sinh tố, xay nhuyễn trong 30 giây."
    else:
        cook_method = "Nấu chín nguyên liệu chính với lửa vừa, đảo đều để thấm gia vị."
    steps.append(f"Bước {step}: {cook_method}")
    step += 1

    # Bước 4: Thêm rau/phụ
    if side_items:
        side_names = [i.name for i in side_items]
        steps.append(f"Bước {step}: Thêm {', '.join(side_names)} vào, đảo đều 2-3 phút cho chín tới.")
        step += 1

    # Bước 5: Nêm nếm
    if spice_items:
        spice_names = [i.name for i in spice_items]
        steps.append(f"Bước {step}: Nêm nếm với {', '.join(spice_names)} cho vừa khẩu vị.")
        step += 1

    # Bước cuối
    steps.append(f"Bước {step}: Tắt bếp, bày ra đĩa và thưởng thức {recipe_name} ngay khi còn nóng!")

    return steps

@ai_router.post("/generate-instructions")
def generate_instructions(req: GenerateInstructionsRequest):
    """
    Sinh hướng dẫn nấu ăn On-demand.
    Luồng chính: Gemini LLM (đầu bếp Việt Nam).
    Fallback: Rule-based template nếu LLM lỗi/offline.
    """
    # ── DEBUG: In payload nhan duoc tu C# ──
    print(f"[DEBUG] === generate-instructions ===")
    print(f"[DEBUG] recipeName: '{req.recipeName}'")
    print(f"[DEBUG] ingredients count: {len(req.ingredients)}")
    for idx, ing in enumerate(req.ingredients):
        print(f"[DEBUG]   [{idx}] name='{ing.name}', qty={ing.quantityGrams}g, role={ing.role}")
    print(f"[DEBUG] _gemini_client={'READY' if _gemini_client else 'NONE'}")

    # ── Try: Gemini LLM ──
    if _gemini_client:
        try:
            if len(req.ingredients) > 0:
                ingredient_text = ", ".join(
                    f"{i.name} ({i.quantityGrams:.0f}g)" for i in req.ingredients
                )
                prompt = (
                    f"Đóng vai một siêu đầu bếp Việt Nam. Hãy viết hướng dẫn nấu món '{req.recipeName}' "
                    f"bằng các nguyên liệu sau: {ingredient_text}.\n\n"
                    f"Yêu cầu:\n"
                    f"- Viết ngắn gọn, tối đa 5 bước\n"
                    f"- Trả về một mảng JSON duy nhất chứa danh sách các chuỗi bước nấu ăn, "
                    f"ví dụ: [\"Bước 1: Chuẩn bị nguyên liệu...\", \"Bước 2: Nấu chín...\"]\n"
                    f"- Tuyệt đối chỉ trả về mảng JSON thô, không bọc trong ký tự markdown ```json và không chứa lời dẫn hay văn bản thừa nào khác."
                )
            else:
                prompt = (
                    f"Hãy đóng vai một siêu đầu bếp Việt Nam. Tôi muốn nấu món '{req.recipeName}'. "
                    f"Hệ thống hiện chưa có định lượng nguyên liệu cụ thể, bạn hãy tự đề xuất nguyên liệu chuẩn nhất "
                    f"và viết hướng dẫn 4-5 bước ngắn gọn để nấu món này.\n\n"
                    f"Yêu cầu:\n"
                    f"- Tự đề xuất các nguyên liệu chuẩn cần có ở các bước đầu\n"
                    f"- Viết cực kỳ ngắn gọn, tổng cộng chỉ từ 4-5 bước\n"
                    f"- Trả về một mảng JSON duy nhất chứa danh sách các chuỗi bước nấu ăn, "
                    f"ví dụ: [\"Bước 1: Chuẩn bị nguyên liệu gồm...\", \"Bước 2: Nấu chín...\"]\n"
                    f"- Tuyệt đối chỉ trả về mảng JSON thô, không bọc trong ký tự markdown ```json và không chứa lời dẫn hay văn bản thừa nào khác."
                )
                
            print(f"[DEBUG] Dang goi Gemini API...")
            response = _gemini_client.models.generate_content(
                model="gemini-flash-latest",
                contents=prompt,
            )
            raw_text = response.text.strip()
            print(f"[DEBUG] Gemini tra ve: {raw_text[:200]}...")

            steps = []
            # Thử parse JSON trực tiếp
            try:
                import json
                import re
                
                # Làm sạch markdown ```json ... ``` hoặc ``` ... ``` nếu có
                clean_text = raw_text
                if clean_text.startswith("```"):
                    lines = clean_text.split("\n")
                    if lines[0].startswith("```"):
                        lines = lines[1:]
                    if lines and lines[-1].startswith("```"):
                        lines = lines[:-1]
                    clean_text = "\n".join(lines).strip()
                
                # Cố gắng tải JSON
                try:
                    steps = json.loads(clean_text)
                except Exception:
                    # Dùng regex trích xuất mảng [...] trong văn bản
                    match = re.search(r'\[\s*".*?"\s*(?:,\s*".*?"\s*)*\]', clean_text, re.DOTALL)
                    if match:
                        steps = json.loads(match.group(0))
            except Exception as json_err:
                print(f"[WARN] Khong the parse JSON tu Gemini: {json_err}. Chuyen sang fallback text parsing.")

            # Nếu parse JSON thất bại hoặc không ra list, dùng fallback parse từng dòng theo định dạng chữ Bước
            if not steps or not isinstance(steps, list):
                lines = [line.strip() for line in raw_text.split("\n") if line.strip()]
                steps = [l for l in lines if l.startswith("Bước") or l.startswith("-") or l.startswith("1.") or l.startswith("2.") or l.startswith("3.") or l.startswith("4.") or l.startswith("5.") or l.startswith("6.")]
                if not steps:
                    steps = [l for l in lines if len(l) > 10][:6]

            if steps:
                print(f"[DEBUG] Gemini OK, {len(steps)} buoc")
                return {"status": "success", "source": "gemini", "steps": steps}
        except Exception as e:
            print(f"[ERROR] Gemini THAT BAI! Type={type(e).__name__}, Detail={e}")

    # ── Catch: Rule-based fallback ──
    print(f"[DEBUG] -> Dung FALLBACK rule-based")
    steps = _fallback_instructions(req.recipeName, req.ingredients)
    return {"status": "success", "source": "fallback", "steps": steps}