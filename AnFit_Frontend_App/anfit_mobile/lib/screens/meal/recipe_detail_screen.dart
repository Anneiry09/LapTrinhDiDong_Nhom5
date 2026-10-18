import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_colors.dart';
import '../../providers/service_providers.dart';

/// ═══════════════════════════════════════════════════════════
/// RecipeDetailScreen — Chi Tiết Công Thức Món Ăn & AI Hướng Dẫn Nấu
/// Tinh gọn, trực quan, hỗ trợ AI sinh các bước nấu ăn theo yêu cầu
/// ═══════════════════════════════════════════════════════════
class RecipeDetailScreen extends ConsumerStatefulWidget {
  final int recipeId;
  final String? recipeName;
  final int? calories;
  final double? protein;
  final double? carbs;
  final double? fat;

  const RecipeDetailScreen({
    super.key,
    required this.recipeId,
    this.recipeName,
    this.calories,
    this.protein,
    this.carbs,
    this.fat,
  });

  @override
  ConsumerState<RecipeDetailScreen> createState() => _RecipeDetailScreenState();
}

class _RecipeDetailScreenState extends ConsumerState<RecipeDetailScreen> {
  bool _loading = true;
  bool _loadingSteps = false;
  Map<String, dynamic>? _recipe;
  List<String> _steps = [];
  final Set<int> _checkedSteps = {};

  @override
  void initState() {
    super.initState();
    _loadRecipe();
  }

  Future<void> _loadRecipe() async {
    setState(() {
      _loading = true;
    });

    Map<String, dynamic>? data;
    if (widget.recipeId > 0) {
      try {
        data = await ref.read(nutritionServiceProvider).getRecipeDetail(widget.recipeId);
      } catch (_) {
        // Nếu không có dữ liệu tĩnh trong DB, tiếp tục dùng thông tin truyền vào để AI sinh hướng dẫn
        data = null;
      }
    }

    final dishName = widget.recipeName ?? data?['name'] ?? 'Món ăn dinh dưỡng';
    final int calo = widget.calories ?? (data?['totalCalories'] as num?)?.round() ?? (data?['calories'] as num?)?.round() ?? 350;
    final double protein = widget.protein ?? (data?['totalProtein'] as num?)?.toDouble() ?? (data?['protein'] as num?)?.toDouble() ?? 25.0;
    final double carbs = widget.carbs ?? (data?['totalCarbs'] as num?)?.toDouble() ?? (data?['carbs'] as num?)?.toDouble() ?? 30.0;
    final double fat = widget.fat ?? (data?['totalFat'] as num?)?.toDouble() ?? (data?['fat'] as num?)?.toDouble() ?? 10.0;
    final int cookingTime = (data?['cookingTimeMinutes'] as num?)?.round() ?? 20;

    final rawIngredients = (data?['ingredients'] as List?)?.cast<Map<String, dynamic>>() ?? [];
    List<Map<String, dynamic>> ingredients = rawIngredients.map((i) {
      return {
        'name': (i['name'] ?? '').toString(),
        'quantityGrams': (i['quantityGrams'] as num?)?.toDouble() ?? 100.0,
        'role': i['role'] ?? 1,
      };
    }).toList();

    if (ingredients.isEmpty) {
      ingredients = [
        {'name': dishName, 'quantityGrams': 150.0, 'role': 1},
        {'name': 'Rau củ & gia vị tự nhiên', 'quantityGrams': 100.0, 'role': 2},
      ];
    }

    final recipeObj = {
      'id': widget.recipeId,
      'name': dishName,
      'calories': calo,
      'protein': protein,
      'carbs': carbs,
      'fat': fat,
      'cookingTimeMinutes': cookingTime,
      'ingredients': ingredients,
    };

    if (mounted) {
      setState(() {
        _recipe = recipeObj;
        _loading = false;
      });
      _loadInstructions(recipeObj);
    }
  }

  Future<void> _loadInstructions(Map<String, dynamic> recipe) async {
    setState(() {
      _loadingSteps = true;
      _checkedSteps.clear();
    });

    final ingredients = (recipe['ingredients'] as List?)?.cast<Map<String, dynamic>>() ?? [];
    final dishName = recipe['name'] ?? widget.recipeName ?? 'Món ăn dinh dưỡng';

    try {
      final result = await ref.read(nutritionServiceProvider).generateInstructions(
        recipeName: dishName,
        ingredients: ingredients,
      );
      if (mounted) {
        final steps = (result['steps'] as List?)?.map((e) => e.toString()).toList() ?? [];
        setState(() {
          _steps = steps.isNotEmpty ? steps : _fallbackSteps(dishName);
          _loadingSteps = false;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _steps = _fallbackSteps(dishName);
          _loadingSteps = false;
        });
      }
    }
  }

  List<String> _fallbackSteps(String name) {
    return [
      'Sơ chế và rửa sạch các nguyên liệu cần thiết cho món $name.',
      'Ướp gia vị thanh nhẹ, hạn chế đường và dầu mỡ theo chuẩn Eat Clean.',
      'Nấu hoặc áp chảo chín đều với lửa vừa trong khoảng 15-20 phút.',
      'Tắt bếp, bày ra đĩa và thưởng thức ngay khi món ăn còn nóng sốt.',
    ];
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final title = widget.recipeName ?? _recipe?['name'] ?? 'Chi Tiết Món Ăn';

    if (_loading) {
      return Scaffold(
        appBar: AppBar(title: Text(title)),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    final r = _recipe ?? {};
    final calo = (r['calories'] as num?)?.round() ?? 350;
    final protein = (r['protein'] as num?)?.toDouble() ?? 25.0;
    final carbs = (r['carbs'] as num?)?.toDouble() ?? 30.0;
    final fat = (r['fat'] as num?)?.toDouble() ?? 10.0;
    final prepMins = r['cookingTimeMinutes'] ?? 20;
    final ingredients = (r['ingredients'] as List?)?.cast<Map<String, dynamic>>() ?? [];

    return Scaffold(
      appBar: AppBar(
        title: Text(title),
        actions: [
          IconButton(
            tooltip: 'Yêu cầu AI tạo lại hướng dẫn',
            icon: _loadingSteps
                ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                : const Icon(Icons.auto_awesome),
            onPressed: _loadingSteps || _recipe == null ? null : () => _loadInstructions(_recipe!),
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // ── CARD THÔNG SỐ DINH DƯỠNG ──
          Card(
            elevation: 0,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
              side: BorderSide(color: cs.outline.withValues(alpha: 0.15)),
            ),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Expanded(
                        child: Text(
                          title,
                          style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 17),
                        ),
                      ),
                      Text(
                        '$calo kcal',
                        style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: cs.primary),
                      ),
                    ],
                  ),
                  const SizedBox(height: 6),
                  Text('⏱️ Thời gian chế biến: $prepMins phút', style: TextStyle(color: Colors.grey[600], fontSize: 13)),
                  const Divider(height: 24),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceAround,
                    children: [
                      _buildNutrient('Đạm', '${protein.toStringAsFixed(0)}g', Colors.red),
                      _buildNutrient('Tinh bột', '${carbs.toStringAsFixed(0)}g', Colors.orange),
                      _buildNutrient('Chất béo', '${fat.toStringAsFixed(0)}g', Colors.amber),
                    ],
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 20),

          // ── NGUYÊN LIỆU (GRAM) ──
          const Text('Nguyên liệu định lượng 🥕', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),

          if (ingredients.isEmpty)
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 8),
              child: Text('Định lượng nguyên liệu tiêu chuẩn chuẩn bị theo khẩu phần.', style: TextStyle(color: Colors.grey[600])),
            )
          else
            ...ingredients.map((ing) {
              final name = ing['name'] ?? 'Nguyên liệu';
              final grams = (ing['quantityGrams'] as num?)?.round() ?? 100;
              return Container(
                margin: const EdgeInsets.only(bottom: 6),
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                decoration: BoxDecoration(
                  color: isDark ? AppColors.darkCard : AppColors.lightCard,
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: cs.outline.withValues(alpha: 0.08)),
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Expanded(child: Text(name, style: const TextStyle(fontWeight: FontWeight.w500))),
                    Text('$grams g', style: TextStyle(fontWeight: FontWeight.bold, color: cs.primary)),
                  ],
                ),
              );
            }),

          const SizedBox(height: 20),

          // ── HƯỚNG DẪN CÁC BƯỚC NẤU BỞI AI (CHECKBOX) ──
          Row(
            children: [
              const Text('AI Hướng dẫn chế biến 👨‍🍳', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
              if (_loadingSteps) ...[
                const SizedBox(width: 10),
                const SizedBox(width: 14, height: 14, child: CircularProgressIndicator(strokeWidth: 2)),
              ],
            ],
          ),
          const SizedBox(height: 8),

          if (_steps.isEmpty && _loadingSteps)
            const Padding(
              padding: EdgeInsets.all(24),
              child: Center(child: Text('Đang tạo hướng dẫn nấu ăn bằng AI...')),
            )
          else
            ...List.generate(_steps.length, (stepIdx) {
              final stepText = _steps[stepIdx];
              final isChecked = _checkedSteps.contains(stepIdx);

              return Card(
                margin: const EdgeInsets.only(bottom: 8),
                elevation: 0,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                  side: BorderSide(
                    color: isChecked ? Colors.green.withValues(alpha: 0.5) : cs.outline.withValues(alpha: 0.1),
                  ),
                ),
                child: CheckboxListTile(
                  value: isChecked,
                  onChanged: (v) {
                    setState(() {
                      if (v == true) {
                        _checkedSteps.add(stepIdx);
                      } else {
                        _checkedSteps.remove(stepIdx);
                      }
                    });
                  },
                  activeColor: Colors.green,
                  title: Text(
                    'Bước ${stepIdx + 1}',
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 13,
                      color: isChecked ? Colors.green : null,
                    ),
                  ),
                  subtitle: Text(
                    stepText,
                    style: TextStyle(
                      decoration: isChecked ? TextDecoration.lineThrough : null,
                      color: isChecked ? Colors.grey : null,
                    ),
                  ),
                ),
              );
            }),
        ],
      ),
    );
  }

  Widget _buildNutrient(String label, String value, Color color) {
    return Column(
      children: [
        Text(value, style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: color)),
        const SizedBox(height: 2),
        Text(label, style: const TextStyle(fontSize: 12, color: Colors.grey)),
      ],
    );
  }
}
