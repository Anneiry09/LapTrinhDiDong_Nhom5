import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../core/theme/app_colors.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';

/// ═══════════════════════════════════════════════════════════
/// RecipeDetailScreen — Chi Tiết Công Thức Món Ăn & Hướng Dẫn Nấu
/// Tinh gọn, rõ ràng, dễ bảo vệ đồ án
/// ═══════════════════════════════════════════════════════════
class RecipeDetailScreen extends ConsumerStatefulWidget {
  final int recipeId;
  final String? recipeName;

  const RecipeDetailScreen({
    super.key,
    required this.recipeId,
    this.recipeName,
  });

  @override
  ConsumerState<RecipeDetailScreen> createState() => _RecipeDetailScreenState();
}

class _RecipeDetailScreenState extends ConsumerState<RecipeDetailScreen> {
  bool _loading = true;
  bool _loadingSteps = false;
  String? _error;
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
      _error = null;
    });
    try {
      final data = await ref.read(nutritionServiceProvider).getRecipeDetail(widget.recipeId);
      if (mounted) {
        setState(() {
          _recipe = data;
          _loading = false;
        });
        _loadInstructions(data);
      }
    } on DioException catch (e) {
      if (mounted) setState(() { _loading = false; _error = ApiClient.errorMessage(e); });
    } catch (_) {
      if (mounted) setState(() { _loading = false; _error = 'Không thể tải công thức.'; });
    }
  }

  Future<void> _loadInstructions(Map<String, dynamic> recipe) async {
    setState(() => _loadingSteps = true);
    final rawIngredients = (recipe['ingredients'] as List?)?.cast<Map<String, dynamic>>() ?? [];
    final ingredients = rawIngredients.map((i) {
      return {
        'name': (i['name'] ?? '').toString(),
        'quantityGrams': (i['quantityGrams'] as num?)?.toDouble() ?? 100.0,
        'role': i['role'] ?? 1,
      };
    }).toList();

    try {
      final result = await ref.read(nutritionServiceProvider).generateInstructions(
        recipeName: recipe['name'] ?? widget.recipeName ?? 'Món ăn',
        ingredients: ingredients,
      );
      if (mounted) {
        setState(() {
          _steps = (result['steps'] as List?)?.map((e) => e.toString()).toList() ?? [];
          _loadingSteps = false;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _steps = [
            'Sơ chế sạch sẽ các nguyên liệu theo định lượng.',
            'Nấu chín nguyên liệu với gia vị vừa ăn, hạn chế dầu mỡ.',
            'Bày ra đĩa và thưởng thức khi còn ấm nóng.',
          ];
          _loadingSteps = false;
        });
      }
    }
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

    if (_error != null) {
      return Scaffold(
        appBar: AppBar(title: Text(title)),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(_error!),
              const SizedBox(height: 12),
              ElevatedButton.icon(
                onPressed: _loadRecipe,
                icon: const Icon(Icons.refresh),
                label: const Text('Thử lại'),
              ),
            ],
          ),
        ),
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
                      Text(
                        title,
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 17),
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
                    Text(name, style: const TextStyle(fontWeight: FontWeight.w500)),
                    Text('$grams g', style: TextStyle(fontWeight: FontWeight.bold, color: cs.primary)),
                  ],
                ),
              );
            }),

          const SizedBox(height: 20),

          // ── HƯỚNG DẪN CÁC BƯỚC NẤU (CHECKBOX) ──
          Row(
            children: [
              const Text('Các bước chế biến 👨‍🍳', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
              if (_loadingSteps) ...[
                const SizedBox(width: 10),
                const SizedBox(width: 14, height: 14, child: CircularProgressIndicator(strokeWidth: 2)),
              ],
            ],
          ),
          const SizedBox(height: 8),

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
