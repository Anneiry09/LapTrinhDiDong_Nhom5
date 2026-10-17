import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import 'recipe_detail_screen.dart';
import '../../widgets/anfit_toast.dart';

/// Category labels for display
const _categoryLabels = {
  1: 'Tinh bột',
  2: 'Protein',
  3: 'Rau',
  4: 'Trái cây',
  5: 'Chất béo',
  6: 'Đồ uống',
  7: 'Gia vị',
};

/// Category icons
const _categoryIcons = {
  1: Icons.rice_bowl_outlined,
  2: Icons.egg_outlined,
  3: Icons.eco_outlined,
  4: Icons.apple,
  5: Icons.water_drop_outlined,
  6: Icons.local_cafe_outlined,
  7: Icons.spa_outlined,
};

/// Màn hình Thực đơn dinh dưỡng — hiển thị macros + Swap/Track
class MealPlanScreen extends ConsumerStatefulWidget {
  final DateOnly? date;
  const MealPlanScreen({super.key, this.date});
  @override
  ConsumerState<MealPlanScreen> createState() => _MealPlanScreenState();
}

/// Lightweight wrapper cho DateOnly (yyyy-MM-dd) để không phụ thuộc package
typedef DateOnly = String;

class _MealPlanScreenState extends ConsumerState<MealPlanScreen> with SingleTickerProviderStateMixin {
  late TabController _tabController;
  bool _loading = true;
  bool _isGenerating = false;
  String? _error;
  Map<String, dynamic>? _plan;
  List<Map<String, dynamic>> _details = [];

  void _showToast(BuildContext context, String message, {required bool isSuccess}) {
    AnFitToast.show(context, message, isError: !isSuccess);
  }

  static const _mealTypes = {1: 'Sáng', 2: 'Trưa', 3: 'Tối', 4: 'Phụ'};
  static const _mealIcons = {
    1: Icons.wb_sunny_outlined,
    2: Icons.wb_cloudy_outlined,
    3: Icons.nightlight_outlined,
    4: Icons.cookie_outlined,
  };

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 4, vsync: this);
    _loadTodayPlan();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadTodayPlan() async {
    setState(() { _loading = true; _error = null; });
    try {
      final svc = ref.read(nutritionServiceProvider);
      final date = widget.date;
      // Nếu có date param → gọi API với query ?date=..., ngược lại gọi getTodayPlan()
      final Map<String, dynamic>? data;
      if (date != null) {
        final res = await svc.getTodayPlan(date: date);
        data = res;
      } else {
        data = await svc.getTodayPlan();
      }
      setState(() {
        _plan = data;
        _details = (data?['mealPlanDetails'] as List?)?.cast<Map<String, dynamic>>() ?? [];
        _loading = false;
      });
    } on DioException catch (e) {
      setState(() { _loading = false; _error = ApiClient.errorMessage(e); });
    } catch (e) {
      setState(() { _loading = false; _error = 'Không thể tải thực đơn.'; });
    }
  }

  void _generateMealPlan() async {
    final hasTracked = _details.any((d) => d['isTracked'] == true);
    if (hasTracked) {
      AnFitToast.show(
        context,
        'Bạn đã ghi nhận bữa ăn hôm nay rồi! Không thể dùng AI tạo mới thực đơn để tránh mất lịch sử ăn uống nhé. 🥰',
        isError: true,
      );
      return;
    }

    setState(() => _isGenerating = true);
    try {
      final today = DateTime.now().toIso8601String().substring(0, 10);
      final data = await ref.read(nutritionServiceProvider).generatePlan(planDate: today);
      setState(() {
        _isGenerating = false;
        _plan = data;
        _details = (data['mealPlanDetails'] as List?)?.cast<Map<String, dynamic>>() ?? [];
      });
      if (mounted) {
        AnFitToast.show(context, 'AI đã thiết kế thực đơn dinh dưỡng mới cho bạn!');
      }
    } on DioException catch (e) {
      if (mounted) {
        setState(() => _isGenerating = false);
        AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isGenerating = false);
        AnFitToast.show(context, 'Lỗi tạo thực đơn, thử lại.', isError: true);
      }
    }
  }

  List<Map<String, dynamic>> _mealsForType(int type) {
    return _details.where((d) => d['mealType'] == type).toList();
  }

  /// Gọi API track (đánh dấu đã ăn)
  Future<void> _trackMeal(Map<String, dynamic> detail) async {
    final planDate = widget.date ?? DateTime.now().toIso8601String().substring(0, 10);
    final todayStr = DateTime.now().toIso8601String().substring(0, 10);
    final isFuture = planDate.compareTo(todayStr) > 0;

    if (isFuture) {
      _showToast(context, '🍽️ Thực đơn này dành cho ngày mai hoặc tương lai. Bạn hãy chuẩn bị nguyên liệu trước và chờ đến ngày đó để thưởng thức và ghi nhận nhé! 🥺', isSuccess: false);
      return;
    }

    final detailId = detail['id'] as int;
    try {
      await ref.read(nutritionServiceProvider).trackMeal(detailId);
      setState(() {
        detail['isTracked'] = true;
      });
      if (mounted) {
        AnFitToast.show(context, 'Chúc bạn ngon miệng! Đã ghi nhận bữa ăn thành công.');
      }
    } on DioException catch (e) {
      if (mounted) {
        AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
      }
    }
  }

  /// Mở bottom sheet chọn món ăn thay thế (Swap)
  Future<void> _showSwapSheet(Map<String, dynamic> detail) async {
    final planDate = widget.date ?? DateTime.now().toIso8601String().substring(0, 10);
    final todayStr = DateTime.now().toIso8601String().substring(0, 10);
    final isFuture = planDate.compareTo(todayStr) > 0;

    if (isFuture) {
      _showToast(context, '🍽️ Thực đơn này dành cho ngày mai hoặc tương lai. Bạn hãy chuẩn bị nguyên liệu trước và chờ đến ngày đó để trao đổi và ghi nhận nhé! 🥺', isSuccess: false);
      return;
    }

    final detailId = detail['id'] as int;
    final mealType = detail['mealType'] as int? ?? 4;
    final currentRecipeId = detail['recipeId'] as int? ?? 0;
    final mealTypeName = _mealTypes[mealType] ?? 'Bữa ăn';

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) => _SwapBottomSheet(
        mealType: mealType,
        mealTypeName: mealTypeName,
        currentRecipeId: currentRecipeId,
        detailId: detailId,
        nutritionService: ref.read(nutritionServiceProvider),
        onSwapped: (newDetail) {
          // Update local state with the swapped data
          setState(() {
            final idx = _details.indexWhere((d) => d['id'] == detailId);
            if (idx != -1) {
              _details[idx] = newDetail;
            }
          });
          // Reload to get updated totals
          _loadTodayPlan();
        },
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return Scaffold(
      appBar: AppBar(
        title: Text(widget.date != null ? 'Thực đơn ${widget.date}' : 'Thực Đơn Dinh Dưỡng'),
        bottom: TabBar(
          controller: _tabController,
          labelColor: cs.primary,
          indicatorColor: cs.primary,
          tabs: [1, 2, 3, 4].map((t) => Tab(
            icon: Icon(_mealIcons[t], size: 18),
            text: _mealTypes[t],
          )).toList(),
        ),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? _buildError(cs)
              : _plan == null
                  ? _buildEmpty(cs, textTheme)
                  : _buildContent(cs, textTheme),
    );
  }

  Widget _buildError(ColorScheme cs) {
    return Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
      Icon(Icons.wifi_off, size: 48, color: cs.error),
      const SizedBox(height: 12),
      Text(_error!, style: TextStyle(color: cs.error)),
      const SizedBox(height: 12),
      ElevatedButton(onPressed: _loadTodayPlan, child: const Text('Thử lại')),
    ]));
  }

  Widget _buildEmpty(ColorScheme cs, TextTheme textTheme) {
    return Column(children: [
      const Spacer(),
      Icon(Icons.restaurant_outlined, size: 56, color: cs.onSurfaceVariant.withValues(alpha: 0.4)),
      const SizedBox(height: 12),
      Text('Chưa có thực đơn hôm nay', style: textTheme.titleMedium?.copyWith(color: cs.onSurfaceVariant)),
      const SizedBox(height: 6),
      Text('Bấm AI Tạo để nhận thực đơn cá nhân hóa!', style: TextStyle(color: cs.onSurfaceVariant)),
      const Spacer(),
      _generateButton(cs),
    ]);
  }

  Widget _buildContent(ColorScheme cs, TextTheme textTheme) {
    final totalCal = (_plan?['totalCalories'] ?? 0).toDouble();
    final totalProtein = (_plan?['totalProtein'] ?? 0).toDouble();
    final totalCarbs = (_plan?['totalCarbs'] ?? 0).toDouble();
    final totalFat = (_plan?['totalFat'] ?? 0).toDouble();
    final totalPrice = (_plan?['totalPrice'] ?? 0);

    final dailyBudget = (_plan?['dailyBudget'] ?? 0.0).toDouble();
    final budgetUsedPercent = (_plan?['budgetUsedPercent'] ?? 0.0).toDouble();
    final budgetWarningsRaw = _plan?['budgetWarnings'];
    final budgetWarnings = budgetWarningsRaw is List
        ? budgetWarningsRaw.map((e) => e.toString()).toList()
        : <String>[];

    return Column(children: [
      // ── Summary bar with macros ──
      Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [cs.primary.withValues(alpha: 0.08), cs.primary.withValues(alpha: 0.02)],
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
          ),
        ),
        child: Column(children: [
          // Total calories
          Row(mainAxisAlignment: MainAxisAlignment.center, children: [
            Icon(Icons.local_fire_department, color: cs.primary, size: 22),
            const SizedBox(width: 6),
            Text('${totalCal.toStringAsFixed(0)} kcal', style: textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold, color: cs.primary)),
            const SizedBox(width: 12),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
              decoration: BoxDecoration(
                color: cs.primary.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Text('${(totalPrice is num ? totalPrice / 1000 : 0).toStringAsFixed(0)}K VNĐ',
                  style: textTheme.bodySmall?.copyWith(color: cs.primary, fontWeight: FontWeight.w600)),
            ),
          ]),
          if (dailyBudget > 0) ...[
            const SizedBox(height: 12),
            // Budget Progress Bar
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      'Ngân sách: ${(totalPrice is num ? totalPrice : 0).toStringAsFixed(0)}₫ / ${dailyBudget.toStringAsFixed(0)}₫',
                      style: textTheme.bodySmall?.copyWith(fontWeight: FontWeight.w600, color: cs.onSurfaceVariant),
                    ),
                    Text(
                      '${budgetUsedPercent.toStringAsFixed(1)}%',
                      style: textTheme.bodySmall?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: budgetUsedPercent > 100
                            ? cs.error
                            : (budgetUsedPercent > 85 ? Colors.orange : cs.primary),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 6),
                ClipRRect(
                  borderRadius: BorderRadius.circular(4),
                  child: LinearProgressIndicator(
                    value: budgetUsedPercent > 100 ? 1.0 : (budgetUsedPercent / 100),
                    minHeight: 6,
                    backgroundColor: cs.outline.withValues(alpha: 0.15),
                    color: budgetUsedPercent > 100
                        ? cs.error
                        : (budgetUsedPercent > 85 ? Colors.orange : cs.primary),
                  ),
                ),
              ],
            ),
          ],
          const SizedBox(height: 12),
          // Macro bars
          Row(children: [
            _macroChip(cs, 'P', totalProtein, const Color(0xFF4CAF50)),
            const SizedBox(width: 8),
            _macroChip(cs, 'C', totalCarbs, const Color(0xFFFFA726)),
            const SizedBox(width: 8),
            _macroChip(cs, 'F', totalFat, const Color(0xFFEF5350)),
          ]),
        ]),
      ),
      // ── Warnings banner (nếu có) ──
      if (budgetWarnings.isNotEmpty)
        Container(
          width: double.infinity,
          margin: const EdgeInsets.fromLTRB(16, 8, 16, 4),
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
          decoration: BoxDecoration(
            color: Colors.amber.withValues(alpha: 0.12),
            border: Border.all(color: Colors.amber.withValues(alpha: 0.4), width: 1),
            borderRadius: BorderRadius.circular(12),
          ),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Icon(Icons.warning_amber_rounded, color: Colors.amber, size: 20),
              const SizedBox(width: 10),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: budgetWarnings.map((warning) => Padding(
                    padding: const EdgeInsets.only(bottom: 2),
                    child: Text(
                      warning,
                      style: textTheme.bodySmall?.copyWith(
                        color: Colors.amber.shade900,
                        fontWeight: FontWeight.w500,
                        height: 1.3,
                      ),
                    ),
                  )).toList(),
                ),
              ),
            ],
          ),
        ),
      // ── Tab content ──
      Expanded(
        child: TabBarView(
          controller: _tabController,
          children: [1, 2, 3, 4].map((type) {
            final meals = _mealsForType(type);
            if (meals.isEmpty) {
              return Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                Icon(_mealIcons[type], size: 40, color: cs.onSurfaceVariant.withValues(alpha: 0.3)),
                const SizedBox(height: 8),
                Text('Không có bữa ${_mealTypes[type]}', style: TextStyle(color: cs.onSurfaceVariant)),
              ]));
            }

            // Calculate per-meal totals
            double mealCal = 0, mealP = 0, mealC = 0, mealF = 0;
            for (final m in meals) {
              mealCal += (m['calculatedCalories'] ?? 0).toDouble();
              mealP += (m['calculatedProtein'] ?? 0).toDouble();
              mealC += (m['calculatedCarbs'] ?? 0).toDouble();
              mealF += (m['calculatedFat'] ?? 0).toDouble();
            }

            return ListView(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
              children: [
                // Per-meal summary
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                  margin: const EdgeInsets.only(bottom: 8),
                  decoration: BoxDecoration(
                    color: cs.surfaceContainerHighest.withValues(alpha: 0.3),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Row(mainAxisAlignment: MainAxisAlignment.spaceAround, children: [
                    _miniStat(context, mealCal.toStringAsFixed(0), 'kcal'),
                    _miniStat(context, '${mealP.toStringAsFixed(1)}g', 'Protein'),
                    _miniStat(context, '${mealC.toStringAsFixed(1)}g', 'Carbs'),
                    _miniStat(context, '${mealF.toStringAsFixed(1)}g', 'Fat'),
                  ]),
                ),
                // Food items
                ...meals.map((m) => _buildFoodCard(m, cs, textTheme)),
              ],
            );
          }).toList(),
        ),
      ),
      // ── Rating Section ──
      _buildRatingSection(cs, textTheme),
      // ── Generate button ──
      _generateButton(cs),
    ]);
  }

  Widget _buildRatingSection(ColorScheme cs, TextTheme textTheme) {
    final planId = _plan?['id'] as int?;
    if (planId == null) return const SizedBox.shrink();

    final currentRating = _plan?['userRating'] as int? ?? 0;

    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: cs.surfaceContainerHighest.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: cs.primary.withValues(alpha: 0.12),
          width: 1,
        ),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.auto_awesome_rounded, color: Colors.amber, size: 16),
              const SizedBox(width: 8),
              Text(
                'Đánh giá thực đơn AI hôm nay',
                style: textTheme.titleSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: cs.onSurface,
                ),
              ),
            ],
          ),
          const SizedBox(height: 4),
          Text(
            'Góp ý của bạn giúp AI cá nhân hóa món ăn tốt hơn!',
            style: textTheme.bodySmall?.copyWith(
              color: cs.onSurfaceVariant,
              fontSize: 11,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 10),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: List.generate(5, (index) {
              final starValue = index + 1;
              final isFilled = starValue <= currentRating;
              return GestureDetector(
                onTap: () async {
                  setState(() {
                    _plan?['userRating'] = starValue;
                  });
                  try {
                    await ref.read(nutritionServiceProvider).rate(planId, rating: starValue);
                    if (mounted) {
                      if (starValue <= 3) {
                        AnFitToast.show(
                          context,
                          'Cảm ơn góp ý! Ban quản trị sẽ rà soát để cải thiện thực đơn này. 🥺',
                        );
                      } else {
                        AnFitToast.show(
                          context,
                          'Cảm ơn bạn đã đánh giá cao thực đơn AI hôm nay! 🥰',
                        );
                      }
                    }
                  } on DioException catch (e) {
                    if (mounted) {
                      AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
                    }
                  } catch (e) {
                    if (mounted) {
                      AnFitToast.show(context, 'Lỗi gửi đánh giá, vui lòng thử lại.', isError: true);
                    }
                  }
                },
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 6),
                  child: Icon(
                    isFilled ? Icons.star_rounded : Icons.star_outline_rounded,
                    color: isFilled ? Colors.amber : cs.onSurfaceVariant.withValues(alpha: 0.3),
                    size: 32,
                  ),
                ),
              );
            }),
          ),
        ],
      ),
    );
  }

  Widget _buildFoodCard(Map<String, dynamic> m, ColorScheme cs, TextTheme textTheme) {
    // Recipe-based: ưu tiên recipeName, fallback foodItem.name
    final recipeName = m['recipeName'] as String?;
    final food = m['foodItem'] as Map<String, dynamic>? ?? {};
    final name = recipeName ?? food['name'] ?? 'Món ăn';
    final unit = recipeName != null ? '1 phần' : (food['unit'] ?? '100g');
    final category = food['category'] as int? ?? 0;
    final categoryName = recipeName != null ? 'Món ăn' : (_categoryLabels[category] ?? '');
    final categoryIcon = recipeName != null ? Icons.restaurant_menu : (_categoryIcons[category] ?? Icons.restaurant);

    final qty = (m['servingSize'] ?? m['quantity'] ?? 0).toDouble();
    final cal = (m['calculatedCalories'] ?? 0).toDouble();
    final protein = (m['calculatedProtein'] ?? 0).toDouble();
    final carbs = (m['calculatedCarbs'] ?? 0).toDouble();
    final fat = (m['calculatedFat'] ?? 0).toDouble();
    final price = m['calculatedPrice'] ?? 0;
    final isTracked = m['isTracked'] == true;

    // Category color mapping
    final catColor = _categoryColor(category);
    final recipeId = m['recipeId'] as int?;

    return GestureDetector(
      onTap: recipeId != null ? () {
        Navigator.of(context).push(MaterialPageRoute(
          builder: (_) => RecipeDetailScreen(recipeId: recipeId, recipeName: recipeName),
        ));
      } : null,
      child: Container(
      margin: const EdgeInsets.only(bottom: 10),
      decoration: BoxDecoration(
        color: cs.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: isTracked
              ? const Color(0xFF4CAF50).withValues(alpha: 0.4)
              : cs.outline.withValues(alpha: 0.1),
          width: isTracked ? 1.5 : 1,
        ),
        boxShadow: [
          BoxShadow(
            color: cs.shadow.withValues(alpha: 0.04),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(children: [
        // ── Header: Icon + Name + Category tag ──
        Padding(
          padding: const EdgeInsets.fromLTRB(14, 14, 14, 0),
          child: Row(children: [
            // Category icon
            Container(
              width: 44, height: 44,
              decoration: BoxDecoration(
                color: catColor.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(categoryIcon, color: catColor, size: 22),
            ),
            const SizedBox(width: 12),
            // Name + category + quantity
            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(name as String, style: textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w700)),
              const SizedBox(height: 3),
              Row(children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
                  decoration: BoxDecoration(
                    color: catColor.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: Text(categoryName, style: TextStyle(fontSize: 10, color: catColor, fontWeight: FontWeight.w600)),
                ),
                const SizedBox(width: 8),
                Text(_formatQuantityAndUnit(qty, unit), style: textTheme.bodySmall?.copyWith(color: cs.onSurfaceVariant)),
              ]),
            ])),
            // Tracked badge
            if (isTracked)
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: const Color(0xFF4CAF50).withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: const Row(mainAxisSize: MainAxisSize.min, children: [
                  Icon(Icons.check_circle, color: Color(0xFF4CAF50), size: 14),
                  SizedBox(width: 4),
                  Text('Đã ăn', style: TextStyle(fontSize: 11, color: Color(0xFF4CAF50), fontWeight: FontWeight.w600)),
                ]),
              ),
          ]),
        ),
        const SizedBox(height: 10),
        // ── Macro row ──
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 14),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
            decoration: BoxDecoration(
              color: cs.surfaceContainerHighest.withValues(alpha: 0.3),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Row(children: [
              _macroItem(Icons.local_fire_department, '${cal.toStringAsFixed(0)}', 'kcal', cs.primary),
              _macroDivider(cs),
              _macroItem(Icons.fitness_center, '${protein.toStringAsFixed(1)}g', 'P', const Color(0xFF4CAF50)),
              _macroDivider(cs),
              _macroItem(Icons.grain, '${carbs.toStringAsFixed(1)}g', 'C', const Color(0xFFFFA726)),
              _macroDivider(cs),
              _macroItem(Icons.opacity, '${fat.toStringAsFixed(1)}g', 'F', const Color(0xFFEF5350)),
              _macroDivider(cs),
              _macroItem(Icons.payments_outlined, '${(price is num ? price / 1000 : 0).toStringAsFixed(0)}K', '₫', cs.onSurfaceVariant),
            ]),
          ),
        ),
        const SizedBox(height: 8),
        // ── Action buttons: Swap & Track ──
        Padding(
          padding: const EdgeInsets.fromLTRB(14, 0, 14, 12),
          child: Row(children: [
            // Swap button
            Expanded(
              child: OutlinedButton.icon(
                onPressed: isTracked ? null : () => _showSwapSheet(m),
                icon: const Icon(Icons.swap_horiz, size: 18),
                label: const Text('Đổi món'),
                style: OutlinedButton.styleFrom(
                  foregroundColor: cs.primary,
                  side: BorderSide(color: cs.primary.withValues(alpha: 0.3)),
                  padding: const EdgeInsets.symmetric(vertical: 8),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  textStyle: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600),
                ),
              ),
            ),
            const SizedBox(width: 10),
            // Track button
            Expanded(
              child: FilledButton.icon(
                onPressed: isTracked ? null : () => _trackMeal(m),
                icon: Icon(isTracked ? Icons.check_circle : Icons.check_circle_outline, size: 18),
                label: Text(isTracked ? 'Đã hoàn tất' : 'Hoàn tất'),
                style: FilledButton.styleFrom(
                  backgroundColor: isTracked ? const Color(0xFF4CAF50).withValues(alpha: 0.2) : const Color(0xFF4CAF50),
                  foregroundColor: isTracked ? const Color(0xFF4CAF50) : Colors.white,
                  disabledBackgroundColor: const Color(0xFF4CAF50).withValues(alpha: 0.12),
                  disabledForegroundColor: const Color(0xFF4CAF50),
                  padding: const EdgeInsets.symmetric(vertical: 8),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  textStyle: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600),
                ),
              ),
            ),
          ]),
        ),
      ]),
      ), // Container
    ); // GestureDetector
  }

  Widget _macroChip(ColorScheme cs, String label, double value, Color color) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 6, horizontal: 8),
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Row(mainAxisAlignment: MainAxisAlignment.center, children: [
          Container(
            width: 8, height: 8,
            decoration: BoxDecoration(color: color, shape: BoxShape.circle),
          ),
          const SizedBox(width: 6),
          Text('$label ${value.toStringAsFixed(1)}g',
              style: TextStyle(fontSize: 12, fontWeight: FontWeight.w700, color: color)),
        ]),
      ),
    );
  }

  Widget _macroItem(IconData icon, String value, String label, Color color) {
    return Expanded(
      child: Column(children: [
        Icon(icon, size: 14, color: color),
        const SizedBox(height: 2),
        Text(value, style: TextStyle(fontSize: 12, fontWeight: FontWeight.w700, color: color)),
        Text(label, style: TextStyle(fontSize: 9, color: color.withValues(alpha: 0.7))),
      ]),
    );
  }

  Widget _macroDivider(ColorScheme cs) {
    return Container(width: 1, height: 28, color: cs.outline.withValues(alpha: 0.1));
  }

  Widget _generateButton(ColorScheme cs) {
    final hasTracked = _details.any((d) => d['isTracked'] == true);
    return Padding(
      padding: const EdgeInsets.all(16),
      child: SizedBox(
        width: double.infinity,
        child: ElevatedButton.icon(
          onPressed: _isGenerating
              ? null
              : () {
                  if (hasTracked) {
                    AnFitToast.show(
                      context,
                      'Bạn đã ghi nhận món ăn rồi! Không thể tạo lại thực đơn mới hôm nay nhé. 🥰',
                      isError: true,
                    );
                  } else {
                    _generateMealPlan();
                  }
                },
          icon: _isGenerating
              ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2))
              : const Icon(Icons.auto_awesome),
          label: Text(_isGenerating ? 'AI đang sinh thực đơn...' : '🤖 AI Tạo Thực Đơn Mới'),
          style: hasTracked
              ? ElevatedButton.styleFrom(
                  backgroundColor: cs.outline.withValues(alpha: 0.12),
                  foregroundColor: cs.onSurfaceVariant.withValues(alpha: 0.4),
                  elevation: 0,
                )
              : null,
        ),
      ),
    );
  }

  Widget _miniStat(BuildContext context, String value, String label) {
    return Column(children: [
      Text(value, style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: Theme.of(context).colorScheme.primary)),
      Text(label, style: TextStyle(fontSize: 10, color: Theme.of(context).colorScheme.onSurfaceVariant)),
    ]);
  }

  String _formatQuantityAndUnit(double qty, String unit) {
    if (unit == '100g' || unit == '100g (nấu chín)') {
      return '${qty.toStringAsFixed(0)}g';
    }
    if (unit == '100ml') {
      return '${qty.toStringAsFixed(0)}ml';
    }

    final double unitCount = qty / 100.0;
    
    if (unit == 'quả') {
      return '${unitCount.toStringAsFixed(1)} quả';
    }
    if (unit == 'bắp') {
      return '${unitCount.toStringAsFixed(1)} bắp';
    }
    if (unit == 'scoop') {
      return '${unitCount.toStringAsFixed(1)} muỗng';
    }
    if (unit == 'hộp 100g') {
      return '${unitCount.toStringAsFixed(1)} hộp';
    }
    if (unit == '250ml') {
      return '${unitCount.toStringAsFixed(1)} ly (250ml)';
    }
    if (unit == '15ml') {
      return '${unitCount.toStringAsFixed(1)} muỗng (15ml)';
    }

    if (qty >= 100) {
      return '${unitCount.toStringAsFixed(1)} $unit';
    }
    return '${qty.toStringAsFixed(0)} $unit';
  }

  Color _categoryColor(int category) {
    switch (category) {
      case 1: return const Color(0xFFFFA726); // Carbs - orange
      case 2: return const Color(0xFFEF5350); // Protein - red
      case 3: return const Color(0xFF66BB6A); // Rau - green
      case 4: return const Color(0xFFAB47BC); // Trái cây - purple
      case 5: return const Color(0xFF42A5F5); // Béo/Hạt - blue
      case 6: return const Color(0xFF26C6DA); // Đồ uống - cyan
      case 7: return const Color(0xFF8D6E63); // Gia vị - brown
      default: return const Color(0xFF78909C);
    }
  }
}

// ═══════════════════════════════════════════════════════════
// Bottom Sheet chọn món ăn thay thế (Swap — Recipe-based)
// ═══════════════════════════════════════════════════════════

class _SwapBottomSheet extends StatefulWidget {
  final int mealType;
  final String mealTypeName;
  final int currentRecipeId;
  final int detailId;
  final dynamic nutritionService;
  final void Function(Map<String, dynamic> newDetail) onSwapped;

  const _SwapBottomSheet({
    required this.mealType,
    required this.mealTypeName,
    required this.currentRecipeId,
    required this.detailId,
    required this.nutritionService,
    required this.onSwapped,
  });

  @override
  State<_SwapBottomSheet> createState() => _SwapBottomSheetState();
}

class _SwapBottomSheetState extends State<_SwapBottomSheet> {
  bool _loading = true;
  bool _swapping = false;
  String? _error;
  List<Map<String, dynamic>> _recipes = [];

  @override
  void initState() {
    super.initState();
    _loadRecipes();
  }

  Future<void> _loadRecipes() async {
    try {
      final data = await widget.nutritionService.getRecipesByMealType(widget.mealType);
      setState(() {
        _recipes = (data as List)
            .cast<Map<String, dynamic>>()
            .where((r) => r['id'] != widget.currentRecipeId)
            .toList();
        _loading = false;
      });
    } catch (e) {
      setState(() {
        _loading = false;
        _error = 'Không thể tải danh sách món ăn.';
      });
    }
  }

  Future<void> _doSwap(int newRecipeId) async {
    setState(() => _swapping = true);
    try {
      final result = await widget.nutritionService.swapMealItem(widget.detailId, newRecipeId);
      widget.onSwapped(result);
      if (mounted) {
        Navigator.of(context).pop();
        AnFitToast.show(context, 'Đã đổi món ăn thành công!');
      }
    } on DioException catch (e) {
      setState(() => _swapping = false);
      if (mounted) {
        AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
      }
    } catch (e) {
      setState(() => _swapping = false);
      if (mounted) {
        AnFitToast.show(context, 'Lỗi đổi món, thử lại.', isError: true);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return Container(
      constraints: BoxConstraints(maxHeight: MediaQuery.of(context).size.height * 0.7),
      decoration: BoxDecoration(
        color: cs.surface,
        borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
      ),
      child: Column(children: [
        // Handle bar
        const SizedBox(height: 12),
        Container(width: 40, height: 4, decoration: BoxDecoration(color: cs.onSurfaceVariant.withValues(alpha: 0.3), borderRadius: BorderRadius.circular(2))),
        const SizedBox(height: 16),
        // Title
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: Row(children: [
            Icon(Icons.swap_horiz, color: cs.primary),
            const SizedBox(width: 8),
            Text('Đổi món ăn', style: textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
            const Spacer(),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
              decoration: BoxDecoration(
                color: cs.primary.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(6),
              ),
              child: Text(widget.mealTypeName, style: TextStyle(fontSize: 12, color: cs.primary, fontWeight: FontWeight.w600)),
            ),
          ]),
        ),
        const SizedBox(height: 4),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: Text('Chọn món ăn thay thế phù hợp bữa "${widget.mealTypeName}"',
              style: textTheme.bodySmall?.copyWith(color: cs.onSurfaceVariant)),
        ),
        const SizedBox(height: 12),
        const Divider(height: 1),
        // Content
        Expanded(
          child: _loading
              ? const Center(child: CircularProgressIndicator())
              : _error != null
                  ? Center(child: Text(_error!, style: TextStyle(color: cs.error)))
                  : _recipes.isEmpty
                      ? Center(child: Text('Không có món ăn thay thế.', style: TextStyle(color: cs.onSurfaceVariant)))
                      : _swapping
                          ? const Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                              CircularProgressIndicator(),
                              SizedBox(height: 12),
                              Text('Đang đổi món ăn...'),
                            ]))
                          : ListView.separated(
                              padding: const EdgeInsets.all(16),
                              itemCount: _recipes.length,
                              separatorBuilder: (_, _) => const SizedBox(height: 8),
                              itemBuilder: (context, i) {
                                final r = _recipes[i];
                                return _buildSwapRecipeItem(r, cs, textTheme);
                              },
                            ),
        ),
      ]),
    );
  }

  Widget _buildSwapRecipeItem(Map<String, dynamic> r, ColorScheme cs, TextTheme textTheme) {
    final name = r['name'] ?? '';
    final cal = r['totalCalories'] ?? 0;
    final protein = r['totalProtein'] ?? 0;
    final carbs = r['totalCarbs'] ?? 0;
    final fat = r['totalFat'] ?? 0;
    final price = r['estimatedPrice'] ?? 0;

    return InkWell(
      onTap: () => _doSwap(r['id'] as int),
      borderRadius: BorderRadius.circular(14),
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: cs.outline.withValues(alpha: 0.1)),
          color: cs.surfaceContainerHighest.withValues(alpha: 0.2),
        ),
        child: Row(children: [
          // Info
          Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text(name as String, style: textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w600)),
            const SizedBox(height: 4),
            Wrap(spacing: 8, runSpacing: 4, children: [
              _swapTag('$cal kcal', cs.primary),
              _swapTag('P:${protein}g', const Color(0xFF4CAF50)),
              _swapTag('C:${carbs}g', const Color(0xFFFFA726)),
              _swapTag('F:${fat}g', const Color(0xFFEF5350)),
            ]),
            const SizedBox(height: 4),
            Text('1 phần • ${(price is num ? price / 1000 : 0).toStringAsFixed(0)}K VNĐ',
                style: textTheme.bodySmall?.copyWith(color: cs.onSurfaceVariant)),
          ])),
          // Arrow
          Icon(Icons.arrow_forward_ios, size: 16, color: cs.primary.withValues(alpha: 0.5)),
        ]),
      ),
    );
  }

  Widget _swapTag(String text, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(4),
      ),
      child: Text(text, style: TextStyle(fontSize: 10, color: color, fontWeight: FontWeight.w600)),
    );
  }
}
