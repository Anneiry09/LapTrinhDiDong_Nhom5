using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using AnFit.Api.Exceptions;
using AnFit.Api.Extensions;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using AnFit.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Implementations.Core
{
    public class WorkoutPlanService : IWorkoutPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiService _aiService;

        public WorkoutPlanService(IUnitOfWork unitOfWork, IAiService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        public async Task<WorkoutPlanResponse?> GetByDateAsync(Guid userId, DateOnly date)
        {
            var plan = await _unitOfWork.WorkoutPlans.GetWithActivitiesAsync(userId, date);
            return plan?.ToResponse();
        }

        public async Task<IEnumerable<WorkoutPlanResponse>> GetRoadmapAsync(Guid userId, DateOnly? startDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            var queryDate = startDate ?? today;

            var plans = await _unitOfWork.WorkoutPlans.FindAsync(w => w.UserId == userId);
            var maxPlan = plans.OrderByDescending(w => w.PlanDate).FirstOrDefault();
            if (maxPlan != null && maxPlan.PlanDate.HasValue)
            {
                var calculatedStartDate = maxPlan.PlanDate.Value.AddDays(-29);
                if (!startDate.HasValue || (startDate.Value >= calculatedStartDate && startDate.Value <= maxPlan.PlanDate.Value))
                {
                    queryDate = calculatedStartDate;
                }
            }

            var endDate = queryDate.AddDays(29);
            var plansList = await _unitOfWork.WorkoutPlans.GetRoadmapWithActivitiesAsync(userId, queryDate, endDate);
            return plansList.Select(p => p.ToResponse()).OrderBy(p => p.PlanDate);
        }

        public async Task<WorkoutPlanResponse> GeneratePlanAsync(Guid userId, GenerateWorkoutPlanRequest request)
        {
            // 1. Xóa plan cũ nếu đã tồn tại cho ngày này
            var existingPlan = await _unitOfWork.WorkoutPlans.GetWithActivitiesAsync(userId, request.PlanDate);
            if (existingPlan != null)
            {
                _unitOfWork.WorkoutPlans.Remove(existingPlan);
                await _unitOfWork.SaveChangesAsync();
            }

            // 2. Lấy thông tin User từ DB
            var user = await _unitOfWork.Users.GetWithDetailsAsync(userId);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");

            // 3. Lấy BodyLog gần nhất để có cân nặng, BMI
            var latestBodyLog = await _unitOfWork.UserBodyLogs.GetLatestByUserAsync(userId);

            double weight = latestBodyLog?.Weight ?? 70;
            double bmi = latestBodyLog?.Bmi ?? 22;

            // Tính tuổi từ DateOfBirth
            double age = 25;
            if (user.DateOfBirth.HasValue)
            {
                age = (DateTime.UtcNow - user.DateOfBirth.Value).TotalDays / 365.25;
            }

            // 4. Lấy toàn bộ Exercises từ DB để làm pool cho AI
            var allExercises = await _unitOfWork.Exercises.GetAllAsync();
            var exerciseList = allExercises.ToList();

            if (exerciseList.Count == 0)
            {
                throw new BadRequestException("Chưa có bài tập nào trong hệ thống. Vui lòng liên hệ Admin.");
            }

            // 5. Lọc bệnh lý - Loại bỏ bài tập nguy hiểm
            var userConditions = user.Conditions;
            var unsafeTargets = GetUnsafeTargets(userConditions);
            var safeExercises = exerciseList
                .Where(ex => !unsafeTargets.Any(t =>
                    (ex.MuscleGroup ?? "").Contains(t, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (safeExercises.Count == 0)
            {
                throw new BadRequestException("Không thể tìm thấy bài tập phù hợp với tình trạng sức khỏe của bạn.");
            }

            // 6. GỌI AI Microservice (Hybrid AI): Chấm điểm + Nhặt bài tập tối ưu
            var aiRequest = new AiCustomSessionRequest
            {
                User = new AiUserContext
                {
                    Age = age,
                    Bmi = bmi,
                    IsMale = (user.Gender ?? 1) == 1 ? 1 : 0,
                    TargetGoal = (int)(user.TargetGoal ?? 0),
                    FitnessLevel = (int)(user.FitnessLevel ?? 0)
                },
                MaxDurationMins = request.MaxDurationMinutes ?? 60,
                WorkoutType = request.WorkoutType ?? 1,
                HasWarmup = request.HasWarmup,
                HasCooldown = request.HasCooldown,
                TargetMuscleGroups = request.TargetMuscleGroups,
                AvailableExercises = safeExercises.Select(ex => new AiAvailableExercise
                {
                    Id = ex.Id,
                    Name = ex.Name ?? "Unknown",
                    BaseCaloriesBurned = ex.BaseCaloriesBurned ?? 0,
                    Type = MapMuscleGroupToType(ex.Name ?? "Unknown", ex.MuscleGroup),
                    MuscleGroup = ex.MuscleGroup ?? "Unknown"
                }).ToList()
            };

            var aiResult = await _aiService.GenerateCustomSessionAsync(aiRequest);

            // 7. Xác định Sets/Reps dựa trên FitnessLevel
            int fitnessLevel = (int)(user.FitnessLevel ?? 0);
            int sets = fitnessLevel switch
            {
                1 => 2, // Beginner: 2 sets
                2 => 3, // Intermediate: 3 sets
                3 => 4, // Advanced: 4 sets
                _ => 3
            };
            int reps = fitnessLevel switch
            {
                1 => 10,
                2 => 12,
                3 => 15,
                _ => 12
            };

            // 8. Tạo WorkoutPlan mới và lưu vào DB
            var newPlan = new WorkoutPlan
            {
                UserId = userId,
                PlanDate = request.PlanDate,
                DifficultyRating = request.DifficultyLevel,
                EstimatedCaloriesBurn = aiResult.TotalEstimatedCalories,
                Status = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.WorkoutPlans.AddAsync(newPlan);
            await _unitOfWork.SaveChangesAsync();

            // 9. Tạo WorkoutActivities từ kết quả AI (bao gồm cả Warm-up và Cool-down)
            foreach (var phase in aiResult.WorkoutPlan)
            {
                var isWarmup = phase.Phase == "Warm-up";
                var isCooldown = phase.Phase == "Cool-down";

                var activity = new WorkoutActivity
                {
                    WorkoutPlanId = newPlan.Id,
                    ExerciseId = phase.ExerciseId,
                    SetsCount = isWarmup || isCooldown ? 1 : (phase.Sets ?? sets),
                    RepsPerSet = isWarmup || isCooldown ? 1 : (phase.Reps ?? reps),
                    RestTimeSeconds = isWarmup ? -10 : (isCooldown ? -20 : (phase.RestTimeSeconds ?? (fitnessLevel == 1 ? 90 : fitnessLevel == 2 ? 60 : 45)))
                };

                await _unitOfWork.WorkoutPlans.AddActivityAsync(activity);
            }

            await _unitOfWork.SaveChangesAsync();

            // 10. Trả về kết quả đầy đủ
            var result = await _unitOfWork.WorkoutPlans.GetWithActivitiesAsync(userId, request.PlanDate);
            return result!.ToResponse();
        }

        public async Task UpdateStatusAsync(int planId, UpdatePlanStatusRequest request)
        {
            var plan = await _unitOfWork.WorkoutPlans.GetByIdAsync(planId);
            if (plan == null) throw new NotFoundException("Không tìm thấy lịch tập.");

            plan.Status = request.Status;
            plan.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.WorkoutPlans.Update(plan);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RatePlanAsync(int planId, RatePlanRequest request)
        {
            var plan = await _unitOfWork.WorkoutPlans.GetByIdAsync(planId);
            if (plan == null) throw new NotFoundException("Không tìm thấy lịch tập.");

            plan.DifficultyRating = request.Rating;
            plan.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.WorkoutPlans.Update(plan);
            await _unitOfWork.SaveChangesAsync();
        }

        // === HELPER METHODS ===

        /// <summary>
        /// Ánh xạ tình trạng bệnh lý thành danh sách nhóm cơ không an toàn.
        /// Ví dụ: Đau lưng → Không nên tập "lower back", "spine".
        /// </summary>
        private static List<string> GetUnsafeTargets(IEnumerable<MedicalCondition>? conditions)
        {
            var unsafeTargets = new List<string>();
            if (conditions == null) return unsafeTargets;

            foreach (var condition in conditions)
            {
                var name = condition.ConditionName?.ToLower() ?? "";

                if (name.Contains("lưng") || name.Contains("back"))
                    unsafeTargets.AddRange(new[] { "lower back", "spine", "erector" });

                if (name.Contains("gối") || name.Contains("knee"))
                    unsafeTargets.AddRange(new[] { "quads", "knees", "calves" });

                if (name.Contains("vai") || name.Contains("shoulder"))
                    unsafeTargets.AddRange(new[] { "delts", "shoulders", "traps" });

                if (name.Contains("tim") || name.Contains("heart") || name.Contains("huyết áp") || name.Contains("blood pressure"))
                    unsafeTargets.AddRange(new[] { "cardiovascular" });
            }

            return unsafeTargets.Distinct().ToList();
        }

        /// <summary>
        /// Ánh xạ Tên bài tập và MuscleGroup text thành Type số để Python AI hiểu.
        /// 1: Bodyweight, 2: Cardio, 3: Dumbbell, 4: Machine, 5: Yoga
        /// </summary>
        private static int MapMuscleGroupToType(string name, string? muscleGroup)
        {
            var combined = ((muscleGroup ?? "") + " " + name).ToLower();
            var nameLower = name.ToLower().Trim();

            // 1. Standalone "ta"/"tạ" or weight keywords checking helper
            bool HasTaWord(string text)
            {
                if (text.Contains("tạ đơn") || text.Contains("ta don") ||
                    text.Contains("tạ đôi") || text.Contains("ta doi") ||
                    text.Contains("tạ ấm") || text.Contains("ta am") ||
                    text.Contains("tạ đòn") || text.Contains("ta don"))
                {
                    return true;
                }
                if (text.Contains("dumbbell") || text.Contains("barbell") || 
                    text.Contains("kettlebell") || text.Contains("db"))
                {
                    return true;
                }

                // Check for whole word "ta" or "tạ" to avoid matching "tay", "taps"
                var separators = new[] { ' ', ',', '.', '(', ')', '[', ']', '-', '+' };
                var words = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                return words.Contains("ta") || words.Contains("tạ");
            }

            // 2. Cardio keyword checking helper (handles "run", "cycle" properly)
            bool HasCardioKeyword(string text)
            {
                if (text.Contains("cardio") || text.Contains("hiit") ||
                    text.Contains("chạy") || text.Contains("chay") ||
                    text.Contains("nhảy") || text.Contains("nhay") ||
                    text.Contains("đạp xe") || text.Contains("dap xe") ||
                    text.Contains("jumping") || text.Contains("burpee") ||
                    text.Contains("sprint") || text.Contains("boxing") ||
                    text.Contains("jacks") || text.Contains("skaters"))
                {
                    // Exclude specific bodyweight exercises
                    if (nameLower == "jump squat" || nameLower == "lunge cơ bản" || 
                        nameLower == "squat cơ bản" || nameLower == "jumping lunges" || 
                        nameLower == "lunge nhảy đổi bên (jumping lunges)" || 
                        nameLower == "plank nhảy rút chân (plank tuck jump)")
                    {
                        return false;
                    }
                    if (nameLower == "mountain climbers")
                    {
                        return false;
                    }
                    return true;
                }

                var separators = new[] { ' ', ',', '.', '(', ')', '[', ']', '-', '+' };
                var words = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (words.Contains("run") || words.Contains("jog") || 
                    words.Contains("bike") || words.Contains("cycle") || 
                    words.Contains("rope") || words.Contains("stair") || 
                    words.Contains("climb"))
                {
                    if (!text.Contains("crunch") && !text.Contains("bicycle") && nameLower != "mountain climbers")
                    {
                        return true;
                    }
                }
                return false;
            }

            // 3. Yoga / Recovery (5)
            bool isCoreException = false;
            var coreKeywords = new[] { "russian twist", "plank", "crunch", "dead bug", "hollow body", "v-up" };
            foreach (var k in coreKeywords)
            {
                if (combined.Contains(k))
                {
                    if (!combined.Contains("pose") && !combined.Contains("tư thế"))
                    {
                        isCoreException = true;
                        break;
                    }
                }
            }

            if (!isCoreException)
            {
                var yogaKeywords = new[]
                {
                    "yoga", "giãn", "gian", "stretch", "pose", "chào mặt trời", "chao mat troi", "salutation",
                    "chó úp mặt", "cho up mat", "downward dog", "cat-cow", "child", "phục hồi", "phuc hoi",
                    "rắn hổ mang", "ran ho mang", "cobra", "chiến binh", "chien binh", "warrior",
                    "bồ câu", "bo cau", "pigeon", "cái cây", "cai cay", "tree", "cây cầu", "cay cau",
                    "trăng lưỡi liềm", "trang luoi liem", "crescent", "em bé", "em be", "baby",
                    "xỏ chỉ", "xo chi", "needle", "lạc đà", "lac da", "camel", "nhân sư", "nhan su", "sphinx",
                    "vũ công", "vu cong", "dancer", "bánh xe", "banh xe", "wheel", "cái cày", "cai cay", "plow",
                    "con cá", "con ca", "fish", "gập người", "gap nguoi", "fold", "vặn người", "van nguoi", "vặn mình", "van minh", "twist",
                    "tam giác", "tam giac", "triangle", "góc nghiêng", "goc nghieng", "extended side angle", "angle",
                    "nửa vầng trăng", "nua vang trang", "half moon", "con mèo", "con meo", "cat pose", "con bò", "con bo", "cow pose",
                    "đại bàng", "dai bang", "eagle", "chiếc ghế", "chiec ghe", "chair pose", "cánh cung", "canh cung", "bow pose",
                    "con quạ", "con qua", "crow pose", "đứng bằng vai", "dung bang vai", "shoulder stand", "savasana",
                    "anh hùng", "anh hung", "hero pose", "locust", "ếch giang chân", "ech giang chan", "frog pose",
                    "cổng sắt", "cong sat", "gate pose", "cánh bướm", "canh buom", "butterfly", "thư giãn", "thu gian", "recovery"
                };

                bool hasYogaKeyword = false;
                foreach (var k in yogaKeywords)
                {
                    if (combined.Contains(k))
                    {
                        hasYogaKeyword = true;
                        break;
                    }
                }

                if (hasYogaKeyword && !combined.Contains("bird dog") && !combined.Contains("cào cào cất cánh"))
                {
                    return 5;
                }
            }

            // 4. Machine (4)
            if (!combined.Contains("bướm") && !combined.Contains("butterfly"))
            {
                var machineKeywords = new[] { "máy", "may", "machine", "cable", "cáp", "smith" };
                bool hasMachineKeyword = false;
                foreach (var k in machineKeywords)
                {
                    if (combined.Contains(k))
                    {
                        hasMachineKeyword = true;
                        break;
                    }
                }

                var machineActionKeywords = new[] { "press", "pulldown", "extension", "fly", "row", "abduction", "adduction" };
                bool hasActionKeyword = false;
                foreach (var k in machineActionKeywords)
                {
                    if (combined.Contains(k))
                    {
                        hasActionKeyword = true;
                        break;
                    }
                }

                if (hasMachineKeyword || (hasActionKeyword && !HasTaWord(combined)))
                {
                    var bodyweightExcludes = new[] { "pull up", "chin up", "push up", "hít đất", "xà đơn", "dips" };
                    bool isBodyweightExclude = false;
                    foreach (var k in bodyweightExcludes)
                    {
                        if (combined.Contains(k))
                        {
                            isBodyweightExclude = true;
                            break;
                        }
                    }

                    if (!isBodyweightExclude)
                    {
                        return 4;
                    }
                }
            }

            // 5. Cardio (2)
            if (HasCardioKeyword(combined))
            {
                return 2;
            }

            // 6. Dumbbell (3)
            bool isDumbbell = HasTaWord(combined);
            if (!isDumbbell)
            {
                var dumbbellKeywords = new[] { "curl", "raise", "deadlift", "shrug" };
                foreach (var k in dumbbellKeywords)
                {
                    if (combined.Contains(k))
                    {
                        isDumbbell = true;
                        break;
                    }
                }
            }

            if (isDumbbell)
            {
                if (combined.Contains("leg raise") || combined.Contains("leg raises"))
                {
                    // Fall through to bodyweight
                }
                else
                {
                    return 3;
                }
            }

            return 1; // Default: Bodyweight
        }

        public async Task Generate30DayRoadmapAsync(Guid userId, bool force = false)
        {
            var random = new Random();
            var user = await _unitOfWork.Users.GetWithDetailsAsync(userId);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");

            // Check xem lộ trình hiện tại đã hoàn thành 100% hay chưa
            var plans = await _unitOfWork.WorkoutPlans.FindAsync(w => w.UserId == userId);
            var maxPlan = plans.OrderByDescending(w => w.PlanDate).FirstOrDefault();
            if (maxPlan != null && maxPlan.PlanDate.HasValue)
            {
                var activeStartDate = maxPlan.PlanDate.Value.AddDays(-29);
                var activeEndDate = maxPlan.PlanDate.Value;
                
                var activePlansList = await _unitOfWork.WorkoutPlans.GetRoadmapWithActivitiesAsync(userId, activeStartDate, activeEndDate);
                var activePlans = activePlansList.ToList();

                // Điều kiện hoàn thành 100%: đủ 30 ngày và tất cả ngày tập đều được hoàn thành (Status == 2), ngày nghỉ (Rest Day - không có WorkoutActivities) được tính hoàn thành mặc định
                bool isCompleted100 = activePlans.Count >= 30 && activePlans.All(p => 
                    p.Status == 2 || 
                    p.WorkoutActivities == null || 
                    p.WorkoutActivities.Count == 0
                );

                if (!isCompleted100 && !force)
                {
                    throw new BadRequestException("ROADMAP_INCOMPLETE");
                }
            }

            var allExercises = (await _unitOfWork.Exercises.GetAllAsync()).ToList();
            if (allExercises.Count == 0) return;

            var userConditions = user.Conditions;
            var unsafeTargets = GetUnsafeTargets(userConditions);
            var safeExercises = allExercises
                .Where(ex => !unsafeTargets.Any(t =>
                    (ex.MuscleGroup ?? "").Contains(t, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (safeExercises.Count == 0)
            {
                safeExercises = allExercises;
            }

            // Pools for dynamic warm-up and cool-down
            var warmupPool = safeExercises.Where(ex => {
                var name = (ex.Name ?? "").ToLower();
                var mg = (ex.MuscleGroup ?? "").ToLower();
                return name.Contains("warm") || name.Contains("khởi động") || name.Contains("xoay") || 
                       name.Contains("inchworm") || name.Contains("cat-cow") || name.Contains("stretch") ||
                       name.Contains("giãn") || mg.Contains("linh hoạt") || mg.Contains("giãn cơ");
            }).ToList();
            if (!warmupPool.Any()) warmupPool = safeExercises;

            var cooldownPool = safeExercises.Where(ex => {
                var name = (ex.Name ?? "").ToLower();
                var mg = (ex.MuscleGroup ?? "").ToLower();
                return name.Contains("cool") || name.Contains("giãn") || name.Contains("stretch") || 
                       name.Contains("pose") || name.Contains("child") || name.Contains("cobra") || 
                       name.Contains("yoga") || name.Contains("cat-cow") || name.Contains("thư giãn") ||
                       mg.Contains("linh hoạt") || mg.Contains("giãn cơ");
            }).ToList();
            if (!cooldownPool.Any()) cooldownPool = safeExercises;

            // Phân tích sở thích hoạt động của người dùng (1: Bodyweight, 2: Cardio, 3: Dumbbell, 4: Machine, 5: Yoga)
            var preferredTypes = new List<int>();
            if (user.PreferredActivities != null && user.PreferredActivities.Any())
            {
                foreach (var pref in user.PreferredActivities)
                {
                    var prefName = (pref.Name ?? "").ToLower();
                    if (prefName.Contains("nhà") || prefName.Contains("home") || prefName.Contains("bodyweight"))
                        preferredTypes.Add(1);
                    if (prefName.Contains("cardio") || prefName.Contains("chạy") || prefName.Contains("hiit") || prefName.Contains("run"))
                        preferredTypes.Add(2);
                    if (prefName.Contains("tạ đơn") || prefName.Contains("dumbbell"))
                        preferredTypes.Add(3);
                    if (prefName.Contains("máy") || prefName.Contains("gym") || prefName.Contains("machine"))
                        preferredTypes.Add(4);
                    if (prefName.Contains("yoga") || prefName.Contains("pilates") || prefName.Contains("giãn") || prefName.Contains("stretch") || prefName.Contains("phục hồi"))
                        preferredTypes.Add(5);
                }
            }

            if (!preferredTypes.Any())
            {
                preferredTypes = new List<int> { 1, 2, 3, 4, 5 };
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            for (int i = 0; i < 30; i++)
            {
                var targetDate = today.AddDays(i);
                var existingPlan = await _unitOfWork.WorkoutPlans.GetWithActivitiesAsync(userId, targetDate);
                if (existingPlan != null)
                {
                    _unitOfWork.WorkoutPlans.Remove(existingPlan);
                }
            }
            await _unitOfWork.SaveChangesAsync();

            int fitnessLevel = (int)(user.FitnessLevel ?? 1);
            int sets = fitnessLevel switch
            {
                1 => 3,
                2 => 3,
                3 => 4,
                _ => 3
            };
            int reps = fitnessLevel switch
            {
                1 => 10,
                2 => 12,
                3 => 15,
                _ => 12
            };
            int rest = fitnessLevel switch
            {
                1 => 90,
                2 => 60,
                3 => 45,
                _ => 60
            };

            int targetGoal = (int)(user.TargetGoal ?? 2);

            for (int i = 0; i < 30; i++)
            {
                int dayIndex = i + 1;
                int dayOfWeek = (i % 7) + 1;
                var targetDate = today.AddDays(i);

                List<string> keywords = new List<string>();
                bool isRecovery = false;

                if (targetGoal == 3) // GainMuscle
                {
                    switch (dayOfWeek)
                    {
                        case 1:
                            keywords = new List<string> { "ngực", "tay sau", "chest", "tricep" };
                            break;
                        case 2:
                            keywords = new List<string> { "lưng", "bắp tay", "back", "bicep" };
                            break;
                        case 3:
                            keywords = new List<string> { "chân", "mông", "leg", "squat", "glute" };
                            break;
                        case 4:
                            keywords = new List<string> { "yoga", "giãn", "stretch", "pose", "child", "cat-cow" };
                            isRecovery = true;
                            break;
                        case 5:
                            keywords = new List<string> { "vai", "bụng", "core", "delt", "shoulder", "abdominal", "abs" };
                            break;
                        case 6:
                            keywords = new List<string> { "cardio", "hiit", "chạy", "nhảy", "plank", "crunch" };
                            break;
                        case 7:
                            keywords = new List<string> { "yoga", "giãn", "stretch", "pose", "child", "cat-cow" };
                            isRecovery = true;
                            break;
                    }
                }
                else if (targetGoal == 1) // LoseWeight
                {
                    switch (dayOfWeek)
                    {
                        case 1:
                            keywords = new List<string> { "cardio", "bụng", "core", "plank", "crunch" };
                            break;
                        case 2:
                            keywords = new List<string> { "chân", "mông", "leg", "squat", "glute", "lunge" };
                            break;
                        case 3:
                            keywords = new List<string> { "cardio", "vai", "hiit", "burpee", "jumping" };
                            break;
                        case 4:
                            keywords = new List<string> { "yoga", "giãn", "stretch", "pose", "child", "cat-cow" };
                            isRecovery = true;
                            break;
                        case 5:
                            keywords = new List<string> { "ngực", "tay sau", "chest", "tricep", "push up" };
                            break;
                        case 6:
                            keywords = new List<string> { "yoga", "giãn", "stretch", "pose", "child", "cat-cow" };
                            isRecovery = true;
                            break;
                        case 7:
                            keywords = new List<string> { "yoga", "giãn", "stretch", "pose", "child", "cat-cow" };
                            isRecovery = true;
                            break;
                    }
                }
                else // MaintainWeight (2)
                {
                    switch (dayOfWeek)
                    {
                        case 1:
                            keywords = new List<string> { "toàn thân", "bodyweight", "inchworm", "bear crawl" };
                            break;
                        case 2:
                            keywords = new List<string> { "yoga", "lưng", "stretch", "pose", "cat-cow", "cobra" };
                            break;
                        case 3:
                            keywords = new List<string> { "bụng", "core", "plank", "crunch", "twist" };
                            break;
                        case 4:
                            keywords = new List<string> { "yoga", "giãn", "stretch", "pose", "child", "cat-cow" };
                            isRecovery = true;
                            break;
                        case 5:
                            keywords = new List<string> { "ngực", "vai", "chest", "shoulder", "push up" };
                            break;
                        case 6:
                            keywords = new List<string> { "chân", "mông", "leg", "squat", "glute", "lunge" };
                            break;
                        case 7:
                            keywords = new List<string> { "yoga", "giãn", "stretch", "pose", "child", "cat-cow" };
                            isRecovery = true;
                            break;
                    }
                }

                // Dynamic exercise count based on Fitness Level & random variation
                int exerciseCount = isRecovery 
                    ? random.Next(3, 5) // 3 or 4 gentle stretching/yoga exercises
                    : fitnessLevel switch
                    {
                        1 => random.Next(3, 5), // 3 or 4 core exercises
                        2 => random.Next(4, 6), // 4 or 5 core exercises
                        3 => random.Next(5, 7), // 5 or 6 core exercises
                        _ => 4
                    };

                // Phân loại danh sách bài tập (dayPool) theo ngày để tránh pha trộn sai nhóm bài tập
                List<Exercise> dayPool = safeExercises;
                if (!isRecovery && dayOfWeek != 6) // Ngày tập Sức mạnh (Strength: Day 1, 2, 3, 5)
                {
                    var strengthTypes = preferredTypes.Intersect(new[] { 1, 3, 4 }).ToList();
                    if (!strengthTypes.Any())
                    {
                        strengthTypes = new List<int> { 1, 3, 4 };
                    }
                    dayPool = safeExercises.Where(ex => strengthTypes.Contains(MapMuscleGroupToType(ex.Name ?? "", ex.MuscleGroup))).ToList();
                    if (!dayPool.Any())
                    {
                        dayPool = safeExercises.Where(ex => new[] { 1, 3, 4 }.Contains(MapMuscleGroupToType(ex.Name ?? "", ex.MuscleGroup))).ToList();
                    }
                }
                else if (dayOfWeek == 6) // Ngày tập Cardio (Day 6)
                {
                    dayPool = safeExercises.Where(ex => MapMuscleGroupToType(ex.Name ?? "", ex.MuscleGroup) == 2).ToList();
                    if (!dayPool.Any())
                    {
                        dayPool = safeExercises.Where(ex => new[] { 1, 2 }.Contains(MapMuscleGroupToType(ex.Name ?? "", ex.MuscleGroup))).ToList();
                    }
                }
                else // Ngày Phục hồi / Yoga (isRecovery = true: Day 4, 7)
                {
                    dayPool = safeExercises.Where(ex => MapMuscleGroupToType(ex.Name ?? "", ex.MuscleGroup) == 5).ToList();
                    if (!dayPool.Any())
                    {
                        dayPool = safeExercises.Where(ex => MapMuscleGroupToType(ex.Name ?? "", ex.MuscleGroup) == 5).ToList();
                    }
                }

                // Dynamic warm-up and cool-down selection for training days
                Exercise? warmupEx = null;
                Exercise? cooldownEx = null;

                if (!isRecovery)
                {
                    warmupEx = warmupPool.OrderBy(x => random.Next()).FirstOrDefault();
                    
                    var corePool = dayPool.Where(ex => warmupEx == null || ex.Id != warmupEx.Id).ToList();
                    var dayExercisesList = GetExercisesByKeywords(corePool, keywords, exerciseCount);
                    
                    cooldownEx = cooldownPool.Where(ex => 
                        (warmupEx == null || ex.Id != warmupEx.Id) && 
                        !dayExercisesList.Any(de => de.Id == ex.Id)
                    ).OrderBy(x => random.Next()).FirstOrDefault();
                }

                var dayExercises = GetExercisesByKeywords(dayPool, keywords, exerciseCount);
                if (!dayExercises.Any()) continue;

                var newPlan = new WorkoutPlan
                {
                    UserId = userId,
                    PlanDate = targetDate,
                    DifficultyRating = 3,
                    Status = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Dynamic sets, reps, rest
                int setsToUse = isRecovery ? 2 : sets;
                int repsToUse = isRecovery ? 8 : reps;
                int restToUse = isRecovery ? 30 : rest;

                // Calorie calculation
                double totalCal = 0;
                if (!isRecovery)
                {
                    if (warmupEx != null) totalCal += (warmupEx.BaseCaloriesBurned ?? 5) * 1;
                    foreach (var ex in dayExercises)
                    {
                        totalCal += (ex.BaseCaloriesBurned ?? 10) * setsToUse;
                    }
                    if (cooldownEx != null) totalCal += (cooldownEx.BaseCaloriesBurned ?? 5) * 1;
                }
                else
                {
                    foreach (var ex in dayExercises)
                    {
                        totalCal += (ex.BaseCaloriesBurned ?? 8) * setsToUse;
                    }
                }
                newPlan.EstimatedCaloriesBurn = totalCal;

                await _unitOfWork.WorkoutPlans.AddAsync(newPlan);
                await _unitOfWork.SaveChangesAsync();

                // Inject Warm-up Activity
                if (!isRecovery && warmupEx != null)
                {
                    var warmupActivity = new WorkoutActivity
                    {
                        WorkoutPlanId = newPlan.Id,
                        ExerciseId = warmupEx.Id,
                        SetsCount = 1,
                        RepsPerSet = 1,
                        RestTimeSeconds = -10 // -10 is Warm-up Rest Time Representation
                    };
                    await _unitOfWork.WorkoutPlans.AddActivityAsync(warmupActivity);
                }

                // Inject Core Workout Activities
                foreach (var ex in dayExercises)
                {
                    var activity = new WorkoutActivity
                    {
                        WorkoutPlanId = newPlan.Id,
                        ExerciseId = ex.Id,
                        SetsCount = setsToUse,
                        RepsPerSet = repsToUse,
                        RestTimeSeconds = restToUse
                    };
                    await _unitOfWork.WorkoutPlans.AddActivityAsync(activity);
                }

                // Inject Cool-down Activity
                if (!isRecovery && cooldownEx != null)
                {
                    var cooldownActivity = new WorkoutActivity
                    {
                        WorkoutPlanId = newPlan.Id,
                        ExerciseId = cooldownEx.Id,
                        SetsCount = 1,
                        RepsPerSet = 1,
                        RestTimeSeconds = -20 // -20 is Cool-down Rest Time Representation
                    };
                    await _unitOfWork.WorkoutPlans.AddActivityAsync(cooldownActivity);
                }

                await _unitOfWork.SaveChangesAsync();
            }
        }

        private static List<Exercise> GetExercisesByKeywords(List<Exercise> pool, List<string> keywords, int count)
        {
            var matched = pool.Where(ex =>
            {
                var name = (ex.Name ?? "").ToLower();
                var mg = (ex.MuscleGroup ?? "").ToLower();
                return keywords.Any(k => name.Contains(k) || mg.Contains(k));
            }).ToList();

            if (matched.Count == 0)
            {
                matched = pool;
            }

            var random = new Random();
            return matched.OrderBy(x => random.Next()).Take(count).ToList();
        }
    }
}