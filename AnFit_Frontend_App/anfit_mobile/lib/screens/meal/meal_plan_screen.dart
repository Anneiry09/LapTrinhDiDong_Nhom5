import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import 'recipe_detail_screen.dart';
import '../../widgets/anfit_toast.dart';

/// ═══════════════════════════════════════════════════════════
/// MealPlanScreen — Thực Đơn Dinh Dưỡng Theo Ngày (Macros + Swap/Track)
/// Tinh gọn, rõ ràng, dễ bảo vệ đồ án
/// ═══════════════════════════════════════════════════════════
class MealPlanScreen extends ConsumerStatefulWidget {
  final String? date;
  const MealPlanScreen({super.key, this.date});

  @override
  ConsumerState<MealPlanScreen> createState() => _MealPlanScreenState();
}

class _MealPlanScreenState extends ConsumerState<MealPlanScreen> {
  bool _loading = true;
  bool _generating = false;
  String? _error;
  List<Map<String, dynamic>> _details = [];

  static const _mealTypes = {1: 'Bữa Sáng ☀️', 2: 'Bữa Trưa 🌤️', 3: 'Bữa Tối 🌙', 4: 'Bữa Phụ 🍪'};

  @override
  void initState() {
    super.initState();
    _loadPlan();
  }

  Future<void> _loadPlan() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final svc = ref.read(nutritionServiceProvider);
      final data = widget.date != null
          ? await svc.getTodayPlan(date: widget.date!)
          : await svc.getTodayPlan();

      if (mounted) {
        setState(() {
          _details = (data?['mealPlanDetails'] as List?)?.cast<Map<String, dynamic>>() ?? [];
          _loading = false;
        });
      }
    } on DioException catch (e) {
      if (mounted) setState(() { _loading = false; _error = ApiClient.errorMessage(e); });
    } catch (_) {
      if (mounted) setState(() { _loading = false; _error = 'Không thể tải thực đơn.'; });
    }
  }

  Future<void> _generateMealPlan() async {
    setState(() => _generating = true);
    try {
      final date = widget.date ?? DateTime.now().toIso8601String().substring(0, 10);
      await ref.read(nutritionServiceProvider).generatePlan(planDate: date);
      if (mounted) AnFitToast.show(context, 'Đã tạo thực đơn ngày mới thành công!');
      await _loadPlan();
    } on DioException catch (e) {
      if (mounted) AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
    } catch (_) {
      if (mounted) AnFitToast.show(context, 'Lỗi tạo thực đơn, thử lại.', isError: true);
    } finally {
      if (mounted) setState(() => _generating = false);
    }
  }

  Future<void> _showSwapBottomSheet(int detailId, int mealType, String currentFoodName) async {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (ctx) {
        return FutureBuilder<List<dynamic>>(
          future: ref.read(nutritionServiceProvider).getRecipesByMealType(mealType),
          builder: (context, snapshot) {
            final cs = Theme.of(context).colorScheme;
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const SizedBox(
                height: 200,
                child: Center(child: CircularProgressIndicator()),
              );
            }
            if (snapshot.hasError || !snapshot.hasData || snapshot.data!.isEmpty) {
              return Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Text('Không có món ăn thay thế khả dụng.', style: TextStyle(fontWeight: FontWeight.bold)),
                    const SizedBox(height: 16),
                    ElevatedButton(onPressed: () => Navigator.pop(ctx), child: const Text('Đóng')),
                  ],
                ),
              );
            }

            final recipes = snapshot.data!;
            return SafeArea(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Đổi món: $currentFoodName', style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                    const SizedBox(height: 4),
                    Text('Chọn món ăn thay thế phù hợp với ${_mealTypes[mealType] ?? "bữa ăn"}:',
                        style: TextStyle(fontSize: 13, color: cs.onSurfaceVariant)),
                    const SizedBox(height: 12),
                    ConstrainedBox(
                      constraints: BoxConstraints(maxHeight: MediaQuery.of(context).size.height * 0.5),
                      child: ListView.separated(
                        shrinkWrap: true,
                        itemCount: recipes.length,
                        separatorBuilder: (_, _) => const Divider(height: 1),
                        itemBuilder: (context, i) {
                          final r = recipes[i];
                          final rId = r['id'] as int;
                          final rName = r['name'] ?? 'Món';
                          final cal = r['totalCalories'] ?? 0;
                          return ListTile(
                            contentPadding: EdgeInsets.zero,
                            title: Text(rName, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
                            subtitle: Text('$cal kcal • P:${r['totalProtein'] ?? 0}g C:${r['totalCarbs'] ?? 0}g F:${r['totalFat'] ?? 0}g',
                                style: const TextStyle(fontSize: 12)),
                            trailing: const Icon(Icons.check_circle_outline, color: Colors.blue),
                            onTap: () async {
                              Navigator.pop(ctx);
                              await _swapItem(detailId, rId);
                            },
                          );
                        },
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
        );
      },
    );
  }

  Future<void> _swapItem(int detailId, int newRecipeId) async {
    try {
      await ref.read(nutritionServiceProvider).swapMealItem(detailId, newRecipeId);
      if (mounted) AnFitToast.show(context, 'Đã đổi món ăn thành công!');
      await _loadPlan();
    } on DioException catch (e) {
      if (mounted) AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
    } catch (_) {
      if (mounted) AnFitToast.show(context, 'Lỗi đổi món, thử lại.', isError: true);
    }
  }

  Future<void> _toggleEaten(Map<String, dynamic> item) async {
    final detailId = item['id'] as int;
    final currentStatus = item['isEaten'] as bool? ?? false;
    final newStatus = !currentStatus;

    setState(() => item['isEaten'] = newStatus);

    try {
      await ref.read(nutritionServiceProvider).trackMeal(detailId);
    } catch (_) {
      if (mounted) {
        setState(() => item['isEaten'] = currentStatus);
        AnFitToast.show(context, 'Lỗi cập nhật trạng thái món ăn.', isError: true);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final displayDate = widget.date ?? 'Hôm nay';

    if (_loading) {
      return Scaffold(
        appBar: AppBar(title: Text('Thực Đơn ($displayDate)')),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    if (_error != null) {
      return Scaffold(
        appBar: AppBar(title: Text('Thực Đơn ($displayDate)')),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(_error!),
              const SizedBox(height: 12),
              ElevatedButton.icon(
                onPressed: _loadPlan,
                icon: const Icon(Icons.refresh),
                label: const Text('Thử lại'),
              ),
            ],
          ),
        ),
      );
    }

    if (_details.isEmpty) {
      return Scaffold(
        appBar: AppBar(title: Text('Thực Đơn ($displayDate)')),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.restaurant_outlined, size: 60, color: Colors.grey),
              const SizedBox(height: 16),
              const Text('Chưa có thực đơn cho ngày này.'),
              const SizedBox(height: 16),
              ElevatedButton.icon(
                onPressed: _generating ? null : _generateMealPlan,
                icon: _generating
                    ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                    : const Icon(Icons.auto_awesome),
                label: const Text('Gợi ý thực đơn bằng AI'),
              ),
            ],
          ),
        ),
      );
    }

    // Tính tổng macros
    int totalCal = 0;
    double totalCarbs = 0;
    double totalProtein = 0;
    double totalFat = 0;

    for (final d in _details) {
      totalCal += ((d['calculatedCalories'] as num?) ?? 0).round();
      final food = d['foodItem'] as Map<String, dynamic>? ?? {};
      totalCarbs += (food['carbs'] as num?)?.toDouble() ?? 0.0;
      totalProtein += (food['protein'] as num?)?.toDouble() ?? 0.0;
      totalFat += (food['fat'] as num?)?.toDouble() ?? 0.0;
    }

    return Scaffold(
      appBar: AppBar(
        title: Text('Thực Đơn ($displayDate)'),
        actions: [
          IconButton(
            tooltip: 'Làm mới thực đơn AI',
            icon: const Icon(Icons.auto_awesome),
            onPressed: _generating ? null : _generateMealPlan,
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 100),
        children: [
          // ── TỔNG QUAN MACROS TRONG NGÀY ──
          Card(
            elevation: 0,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
              side: BorderSide(color: cs.outline.withValues(alpha: 0.15)),
            ),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text('Tổng Calo Nạp Vào', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                      Text('$totalCal kcal', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: cs.primary)),
                    ],
                  ),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      _buildMacroTag('Tinh bột', '${totalCarbs.toStringAsFixed(0)}g', Colors.orange),
                      const SizedBox(width: 8),
                      _buildMacroTag('Đạm (Protein)', '${totalProtein.toStringAsFixed(0)}g', Colors.red),
                      const SizedBox(width: 8),
                      _buildMacroTag('Chất béo', '${totalFat.toStringAsFixed(0)}g', Colors.amber),
                    ],
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),

          // ── CÁC BỮA ĂN (SÁNG - TRƯA - TỐI - PHỤ) ──
          ...[1, 2, 3, 4].map((mealType) {
            final items = _details.where((d) => (d['mealType'] as int? ?? 1) == mealType).toList();
            if (items.isEmpty) return const SizedBox.shrink();

            final mealTitle = _mealTypes[mealType] ?? 'Bữa ăn';

            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Padding(
                  padding: const EdgeInsets.symmetric(vertical: 8),
                  child: Text(
                    mealTitle,
                    style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                ),
                ...items.map((item) {
                  final food = item['foodItem'] as Map<String, dynamic>? ?? {};
                  final name = food['name'] ?? 'Món ăn';
                  final cal = ((item['calculatedCalories'] as num?) ?? 0).round();
                  final isEaten = item['isEaten'] as bool? ?? false;
                  final detailId = item['id'] as int;
                  final recipeId = food['recipeId'] as int?;

                  return Card(
                    margin: const EdgeInsets.only(bottom: 8),
                    elevation: 0,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14),
                      side: BorderSide(
                        color: isEaten ? Colors.green.withValues(alpha: 0.5) : cs.outline.withValues(alpha: 0.1),
                      ),
                    ),
                    child: ListTile(
                      onTap: () {
                        if (recipeId != null) {
                          Navigator.push(
                            context,
                            MaterialPageRoute(
                              builder: (_) => RecipeDetailScreen(recipeId: recipeId, recipeName: name),
                            ),
                          );
                        } else {
                          AnFitToast.show(context, 'Món này không có công thức nấu ăn.');
                        }
                      },
                      leading: Checkbox(
                        value: isEaten,
                        onChanged: (_) => _toggleEaten(item),
                        activeColor: Colors.green,
                      ),
                      title: Text(
                        name,
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 14,
                          decoration: isEaten ? TextDecoration.lineThrough : null,
                        ),
                      ),
                      subtitle: Text(
                        '$cal kcal  ${recipeId != null ? "• 📖 Xem công thức" : ""}',
                        style: TextStyle(fontSize: 12, color: isDark ? Colors.white70 : Colors.black54),
                      ),
                      trailing: IconButton(
                        tooltip: 'Đổi món khác',
                        icon: const Icon(Icons.swap_horiz_rounded, color: Colors.blue),
                        onPressed: () => _showSwapBottomSheet(detailId, mealType, name),
                      ),
                    ),
                  );
                }),
                const SizedBox(height: 8),
              ],
            );
          }),
        ],
      ),
    );
  }

  Widget _buildMacroTag(String label, String value, Color color) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 8),
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(10),
        ),
        child: Column(
          children: [
            Text(value, style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: color)),
            const SizedBox(height: 2),
            Text(label, style: const TextStyle(fontSize: 10, color: Colors.grey)),
          ],
        ),
      ),
    );
  }
}
