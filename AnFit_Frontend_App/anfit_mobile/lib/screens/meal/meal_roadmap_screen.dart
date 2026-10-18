import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import 'recipe_detail_screen.dart';
import '../../widgets/anfit_toast.dart';

/// ═══════════════════════════════════════════════════════════
/// MealRoadmapScreen — Màn hình Dinh Dưỡng & Thực Đơn Theo Ngày
/// Tinh gọn, trực quan, tập trung tính năng cốt lõi (dễ bảo vệ đồ án)
/// ═══════════════════════════════════════════════════════════
class MealRoadmapScreen extends ConsumerStatefulWidget {
  final String? initialDate;
  const MealRoadmapScreen({super.key, this.initialDate});

  @override
  ConsumerState<MealRoadmapScreen> createState() => _MealRoadmapScreenState();
}

class _MealRoadmapScreenState extends ConsumerState<MealRoadmapScreen> {
  late DateTime _selectedDate;
  bool _loading = true;
  bool _generating = false;
  String? _error;
  List<Map<String, dynamic>> _details = [];

  static const _mealTitles = {
    1: 'Bữa Sáng ☀️',
    2: 'Bữa Trưa 🌤️',
    3: 'Bữa Tối 🌙',
    4: 'Bữa Phụ 🍪',
  };

  @override
  void initState() {
    super.initState();
    if (widget.initialDate != null) {
      _selectedDate = DateTime.tryParse(widget.initialDate!) ?? DateTime.now();
    } else {
      _selectedDate = DateTime.now();
    }
    _loadPlan();
  }

  String get _dateParam => _selectedDate.toIso8601String().substring(0, 10);

  String _formatDisplayDate(DateTime d) {
    final now = DateTime.now();
    final isToday = d.year == now.year && d.month == now.month && d.day == now.day;
    final formatted = '${d.day.toString().padLeft(2, '0')}/${d.month.toString().padLeft(2, '0')}/${d.year}';
    return isToday ? 'Hôm nay ($formatted)' : formatted;
  }

  // 1. Tải thực đơn theo ngày
  Future<void> _loadPlan() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final svc = ref.read(nutritionServiceProvider);
      final data = await svc.getTodayPlan(date: _dateParam);
      if (mounted) {
        setState(() {
          _details = (data?['mealPlanDetails'] as List?)?.cast<Map<String, dynamic>>() ?? [];
          _loading = false;
        });
      }
    } on DioException catch (e) {
      if (mounted) setState(() { _loading = false; _error = ApiClient.errorMessage(e); });
    } catch (_) {
      if (mounted) setState(() { _loading = false; _error = 'Không thể tải dữ liệu thực đơn.'; });
    }
  }

  // 2. Gợi ý thực đơn mới bằng AI
  Future<void> _generatePlan() async {
    setState(() => _generating = true);
    try {
      await ref.read(nutritionServiceProvider).generatePlan(planDate: _dateParam);
      if (mounted) AnFitToast.show(context, 'Đã tạo thực đơn ngày mới bằng AI thành công!');
      await _loadPlan();
    } on DioException catch (e) {
      if (mounted) AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
    } catch (_) {
      if (mounted) AnFitToast.show(context, 'Lỗi tạo thực đơn, vui lòng thử lại.', isError: true);
    } finally {
      if (mounted) setState(() => _generating = false);
    }
  }

  // 3. Đổi ngày xem thực đơn
  void _changeDate(int offsetDays) {
    setState(() {
      _selectedDate = _selectedDate.add(Duration(days: offsetDays));
    });
    _loadPlan();
  }

  // 4. Chọn ngày trên lịch
  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime.now().subtract(const Duration(days: 365)),
      lastDate: DateTime.now().add(const Duration(days: 365)),
    );
    if (picked != null && mounted) {
      setState(() => _selectedDate = picked);
      _loadPlan();
    }
  }

  // 5. Đánh dấu đã ăn / chưa ăn
  Future<void> _toggleEaten(Map<String, dynamic> item) async {
    final detailId = item['id'] as int;
    final currentStatus = item['isEaten'] as bool? ?? false;
    setState(() => item['isEaten'] = !currentStatus);

    try {
      await ref.read(nutritionServiceProvider).trackMeal(detailId);
    } catch (_) {
      if (mounted) {
        setState(() => item['isEaten'] = currentStatus);
        AnFitToast.show(context, 'Lỗi cập nhật trạng thái bữa ăn.', isError: true);
      }
    }
  }

  // 6. Đổi món ăn (Swap meal)
  Future<void> _showSwapDialog(int detailId, int mealType, String currentName) async {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (ctx) {
        final cs = Theme.of(ctx).colorScheme;
        return FutureBuilder<List<dynamic>>(
          future: ref.read(nutritionServiceProvider).getRecipesByMealType(mealType),
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const SizedBox(
                height: 180,
                child: Center(child: CircularProgressIndicator()),
              );
            }
            if (snapshot.hasError || !snapshot.hasData || snapshot.data!.isEmpty) {
              return Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Text('Chưa có món ăn thay thế khả dụng.', style: TextStyle(fontWeight: FontWeight.bold)),
                    const SizedBox(height: 12),
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
                    Text('Đổi món: $currentName', style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                    const SizedBox(height: 4),
                    Text('Chọn món ăn thay thế phù hợp với ${_mealTitles[mealType] ?? "bữa ăn"}:',
                        style: TextStyle(fontSize: 13, color: cs.onSurfaceVariant)),
                    const SizedBox(height: 12),
                    ConstrainedBox(
                      constraints: BoxConstraints(maxHeight: MediaQuery.of(context).size.height * 0.45),
                      child: ListView.separated(
                        shrinkWrap: true,
                        itemCount: recipes.length,
                        separatorBuilder: (_, _) => const Divider(height: 1),
                        itemBuilder: (itemCtx, i) {
                          final r = recipes[i];
                          final rId = r['id'] as int;
                          final rName = r['name'] ?? 'Món';
                          final cal = r['totalCalories'] ?? 0;
                          return ListTile(
                            contentPadding: EdgeInsets.zero,
                            title: Text(rName, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
                            subtitle: Text('$cal kcal • Đạm:${r['totalProtein'] ?? 0}g Tinh bột:${r['totalCarbs'] ?? 0}g Béo:${r['totalFat'] ?? 0}g',
                                style: const TextStyle(fontSize: 12)),
                            trailing: const Icon(Icons.check_circle_outline, color: Colors.blue),
                            onTap: () async {
                              Navigator.pop(ctx);
                              try {
                                await ref.read(nutritionServiceProvider).swapMealItem(detailId, rId);
                                if (mounted) {
                                  AnFitToast.show(this.context, 'Đã đổi món thành công!');
                                  _loadPlan();
                                }
                              } on DioException catch (e) {
                                if (mounted) AnFitToast.show(this.context, ApiClient.errorMessage(e), isError: true);
                              } catch (_) {
                                if (mounted) AnFitToast.show(this.context, 'Lỗi đổi món, thử lại.', isError: true);
                              }
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

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final isDark = Theme.of(context).brightness == Brightness.dark;

    // Tính tổng macros trong ngày
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
        title: const Text('Dinh Dưỡng'),
        actions: [
          IconButton(
            tooltip: 'Tạo lại thực đơn bằng AI',
            icon: _generating
                ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                : const Icon(Icons.auto_awesome),
            onPressed: _generating ? null : _generatePlan,
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 110),
        children: [
          // ── THANH CHỌN NGÀY (DATE SELECTOR) ──
          Card(
            elevation: 0,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
              side: BorderSide(color: cs.outline.withValues(alpha: 0.15)),
            ),
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              child: Row(
                children: [
                  IconButton(
                    tooltip: 'Ngày trước',
                    icon: const Icon(Icons.chevron_left_rounded),
                    onPressed: () => _changeDate(-1),
                  ),
                  Expanded(
                    child: InkWell(
                      borderRadius: BorderRadius.circular(8),
                      onTap: _pickDate,
                      child: Padding(
                        padding: const EdgeInsets.symmetric(vertical: 8),
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Icon(Icons.calendar_today_rounded, size: 16),
                            const SizedBox(width: 8),
                            Text(
                              _formatDisplayDate(_selectedDate),
                              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                  IconButton(
                    tooltip: 'Ngày sau',
                    icon: const Icon(Icons.chevron_right_rounded),
                    onPressed: () => _changeDate(1),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 12),

          // ── NỘI DUNG CHÍNH (LOADING / ERROR / EMPTY / DATA) ──
          if (_loading)
            const Padding(
              padding: EdgeInsets.all(40),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_error != null)
            Card(
              margin: const EdgeInsets.symmetric(vertical: 20),
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  children: [
                    const Icon(Icons.error_outline, size: 40, color: Colors.redAccent),
                    const SizedBox(height: 12),
                    Text(_error!, textAlign: TextAlign.center),
                    const SizedBox(height: 16),
                    ElevatedButton.icon(
                      onPressed: _loadPlan,
                      icon: const Icon(Icons.refresh),
                      label: const Text('Thử lại'),
                    ),
                  ],
                ),
              ),
            )
          else if (_details.isEmpty)
            Card(
              elevation: 0,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
                side: BorderSide(color: cs.outline.withValues(alpha: 0.15)),
              ),
              child: Padding(
                padding: const EdgeInsets.all(28),
                child: Column(
                  children: [
                    Icon(Icons.restaurant_outlined, size: 56, color: cs.primary.withValues(alpha: 0.7)),
                    const SizedBox(height: 12),
                    const Text('Chưa có thực đơn cho ngày này', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                    const SizedBox(height: 6),
                    Text(
                      'Nhấn nút bên dưới để AI tự động tính toán và tạo thực đơn dinh dưỡng chuẩn calo cho bạn.',
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 13, color: cs.onSurfaceVariant),
                    ),
                    const SizedBox(height: 18),
                    ElevatedButton.icon(
                      onPressed: _generating ? null : _generatePlan,
                      icon: _generating
                          ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                          : const Icon(Icons.auto_awesome),
                      label: const Text('Gợi ý thực đơn bằng AI'),
                    ),
                  ],
                ),
              ),
            )
          else ...[
            // ── THẺ TỔNG HỢP CALO & MACROS ──
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

            // ── CÁC BỮA ĂN (SÁNG, TRƯA, TỐI, PHỤ) ──
            ...[1, 2, 3, 4].map((mealType) {
              final items = _details.where((d) => (d['mealType'] as int? ?? 1) == mealType).toList();
              if (items.isEmpty) return const SizedBox.shrink();

              final title = _mealTitles[mealType] ?? 'Bữa ăn';

              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Padding(
                    padding: const EdgeInsets.symmetric(vertical: 6),
                    child: Text(title, style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold)),
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
                          '$cal kcal  ${recipeId != null ? "• 👨‍🍳 Cách nấu (AI)" : ""}',
                          style: TextStyle(fontSize: 12, color: isDark ? Colors.white70 : Colors.black54),
                        ),
                        trailing: IconButton(
                          tooltip: 'Đổi món khác',
                          icon: const Icon(Icons.swap_horiz_rounded, color: Colors.blue),
                          onPressed: () => _showSwapDialog(detailId, mealType, name),
                        ),
                      ),
                    );
                  }),
                  const SizedBox(height: 6),
                ],
              );
            }),
          ],
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
