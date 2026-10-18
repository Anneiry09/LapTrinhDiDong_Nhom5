import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/theme_provider.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../workout/workout_plan_screen.dart';
import '../meal/meal_roadmap_screen.dart';
import '../settings/settings_screen.dart';
import '../auth/entry_screen.dart';

/// ═══════════════════════════════════════════════════════════
/// DashboardScreen — Shell chính với Bottom Navigation Bar 4 Tabs
/// ═══════════════════════════════════════════════════════════
class DashboardScreen extends ConsumerStatefulWidget {
  const DashboardScreen({super.key});

  @override
  ConsumerState<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends ConsumerState<DashboardScreen> {
  int _currentIndex = 0;

  void _switchTab(int index) {
    setState(() => _currentIndex = index);
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    final screens = [
      _HomeTab(key: const ValueKey('tab_home'), onNavigateTab: _switchTab),
      const MealRoadmapScreen(key: ValueKey('tab_meal')),
      const WorkoutPlanScreen(key: ValueKey('tab_workout')),
      const SettingsScreen(key: ValueKey('tab_settings')),
    ];

    return Scaffold(
      body: IndexedStack(index: _currentIndex, children: screens),
      extendBody: true,
      bottomNavigationBar: _PremiumNavBar(
        currentIndex: _currentIndex,
        isDark: isDark,
        onTap: _switchTab,
      ),
    );
  }
}

// ─── Bottom Navigation Bar ────────────────────────────────────────────────────
class _PremiumNavBar extends StatelessWidget {
  final int currentIndex;
  final bool isDark;
  final ValueChanged<int> onTap;

  const _PremiumNavBar({
    required this.currentIndex,
    required this.isDark,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final bg = isDark ? AppColors.darkSurface : AppColors.lightSurface;
    const items = [
      (Icons.home_rounded, Icons.home_outlined, 'Trang chủ'),
      (Icons.restaurant_rounded, Icons.restaurant_outlined, 'Dinh dưỡng'),
      (Icons.fitness_center_rounded, Icons.fitness_center_outlined, 'Tập luyện'),
      (Icons.settings_rounded, Icons.settings_outlined, 'Cài đặt'),
    ];

    return ClipRRect(
      child: BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 20, sigmaY: 20),
        child: Container(
          padding: const EdgeInsets.only(bottom: 6),
          decoration: BoxDecoration(
            color: bg.withValues(alpha: 0.90),
            border: Border(
              top: BorderSide(
                color: isDark ? Colors.white.withValues(alpha: 0.08) : Colors.black.withValues(alpha: 0.06),
                width: 0.8,
              ),
            ),
          ),
          child: SafeArea(
            top: false,
            child: Row(
              children: List.generate(items.length, (i) {
                final selected = currentIndex == i;
                final color = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
                final mutedColor = isDark ? AppColors.darkTextSub : AppColors.lightTextSub;
                return Expanded(
                  child: GestureDetector(
                    onTap: () => onTap(i),
                    behavior: HitTestBehavior.opaque,
                    child: Container(
                      padding: const EdgeInsets.symmetric(vertical: 8),
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 4),
                            decoration: BoxDecoration(
                              color: selected ? color.withValues(alpha: 0.15) : Colors.transparent,
                              borderRadius: BorderRadius.circular(12),
                            ),
                            child: Icon(
                              selected ? items[i].$1 : items[i].$2,
                              color: selected ? color : mutedColor,
                              size: 22,
                            ),
                          ),
                          const SizedBox(height: 3),
                          Text(
                            items[i].$3,
                            style: GoogleFonts.inter(
                              fontSize: 10.5,
                              fontWeight: selected ? FontWeight.w600 : FontWeight.w400,
                              color: selected ? color : mutedColor,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                );
              }),
            ),
          ),
        ),
      ),
    );
  }
}

// ═══════════════════════════════════════════════════════════
// TAB TRANG CHỦ — Tinh gọn, rõ ràng, dễ bảo vệ đồ án
// ═══════════════════════════════════════════════════════════
class _HomeTab extends ConsumerStatefulWidget {
  final ValueChanged<int> onNavigateTab;
  const _HomeTab({super.key, required this.onNavigateTab});

  @override
  ConsumerState<_HomeTab> createState() => _HomeTabState();
}

class _HomeTabState extends ConsumerState<_HomeTab> {
  Map<String, dynamic>? _profile;
  Map<String, dynamic>? _dashboard;
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final userSvc = ref.read(userServiceProvider);
      final profile = await userSvc.getProfile();
      Map<String, dynamic> dashboard = {};
      try {
        dashboard = await userSvc.getDashboard();
      } catch (_) {}

      if (mounted) {
        setState(() {
          _profile = profile;
          _dashboard = dashboard;
          _loading = false;
        });
      }
    } on DioException catch (e) {
      if (mounted) {
        setState(() {
          _loading = false;
          _error = ApiClient.errorMessage(e);
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _loading = false;
          _error = 'Không thể tải dữ liệu trang chủ.';
        });
      }
    }
  }

  Future<void> _confirmLogout(BuildContext context) async {
    final cs = Theme.of(context).colorScheme;
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Đăng xuất'),
        content: const Text('Bạn có chắc chắn muốn đăng xuất tài khoản?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: ElevatedButton.styleFrom(backgroundColor: cs.error, foregroundColor: Colors.white),
            child: const Text('Đăng xuất'),
          ),
        ],
      ),
    );

    if (confirm == true && context.mounted) {
      await ref.read(authServiceProvider).logout();
      if (context.mounted) {
        Navigator.pushAndRemoveUntil(
          context,
          MaterialPageRoute(builder: (_) => const EntryScreen()),
          (_) => false,
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    if (_loading) {
      return Container(
        color: isDark ? AppColors.darkBg : AppColors.lightBg,
        child: const Center(child: CircularProgressIndicator()),
      );
    }

    if (_error != null) {
      return Container(
        color: isDark ? AppColors.darkBg : AppColors.lightBg,
        child: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(Icons.cloud_off_rounded, size: 48, color: AppColors.error.withValues(alpha: 0.7)),
              const SizedBox(height: 12),
              Text(_error!, style: TextStyle(color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub)),
              const SizedBox(height: 16),
              ElevatedButton.icon(
                onPressed: _loadData,
                icon: const Icon(Icons.refresh),
                label: const Text('Thử lại'),
              ),
            ],
          ),
        ),
      );
    }

    final p = _profile ?? {};
    final d = _dashboard ?? {};

    final userName = p['fullName'] ?? 'Người dùng';
    final currentWeight = (d['currentWeight'] ?? p['weightKg'] ?? 0.0).toDouble();
    final heightCm = (p['heightCm'] as num?)?.toDouble() ?? 170.0;
    final bmi = (heightCm > 0 && currentWeight > 0)
        ? currentWeight / ((heightCm / 100) * (heightCm / 100))
        : 0.0;

    final targetGoal = p['targetGoal'] ?? 1;
    final targetGoalName = targetGoal == 1
        ? 'Giảm cân & Đốt mỡ'
        : (targetGoal == 2 ? 'Duy trì vóc dáng' : 'Tăng cơ & Săn chắc');

    // Thể trạng & Calo
    final tdee = (d['todayTdee'] as num?)?.round() ?? 2000;
    final todayMeal = d['todayMealPlan'] as Map<String, dynamic>? ?? {};
    final todayWorkout = d['todayWorkoutPlan'] as Map<String, dynamic>? ?? {};

    // Calo món ăn
    final mealDetails = (todayMeal['mealPlanDetails'] as List?)?.cast<Map<String, dynamic>>() ?? [];
    final consumedCal = mealDetails.fold<int>(0, (sum, item) {
      return sum + ((item['calculatedCalories'] as num?) ?? 0).round();
    });

    final hour = DateTime.now().hour;
    final greeting = hour < 12 ? '☀️ Chào buổi sáng' : (hour < 18 ? '🌤️ Chào buổi chiều' : '🌙 Chào buổi tối');

    return Container(
      color: isDark ? AppColors.darkBg : AppColors.lightBg,
      child: SafeArea(
        child: RefreshIndicator(
          onRefresh: _loadData,
          color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary,
          child: SingleChildScrollView(
            physics: const AlwaysScrollableScrollPhysics(parent: BouncingScrollPhysics()),
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 110),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // ── HEADER ──
                Row(
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            greeting,
                            style: GoogleFonts.inter(
                              fontSize: 13,
                              color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            userName,
                            style: GoogleFonts.outfit(
                              fontSize: 22,
                              fontWeight: FontWeight.bold,
                              color: isDark ? AppColors.darkText : AppColors.lightText,
                            ),
                          ),
                        ],
                      ),
                    ),
                    Consumer(
                      builder: (ctx, watchRef, _) => IconButton(
                        tooltip: isDark ? 'Chuyển sang giao diện sáng' : 'Chuyển sang giao diện tối',
                        onPressed: () => watchRef.read(themeModeProvider.notifier).toggleTheme(),
                        icon: Icon(isDark ? Icons.light_mode_outlined : Icons.dark_mode_outlined, size: 22),
                        style: IconButton.styleFrom(
                          backgroundColor: isDark ? AppColors.darkCard : AppColors.lightCard,
                        ),
                      ),
                    ),
                    const SizedBox(width: 8),
                    IconButton(
                      tooltip: 'Đăng xuất',
                      onPressed: () => _confirmLogout(context),
                      icon: const Icon(Icons.logout_rounded, color: AppColors.error, size: 22),
                      style: IconButton.styleFrom(
                        backgroundColor: isDark ? AppColors.darkCard : AppColors.lightCard,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 20),

                // ── THẺ CHỈ SỐ SỨC KHỎE (HERO CARD) ──
                _buildHeroCard(
                  isDark: isDark,
                  weight: currentWeight,
                  heightCm: heightCm,
                  bmi: bmi,
                  goalName: targetGoalName,
                  tdee: tdee,
                  consumedCal: consumedCal,
                ),
                const SizedBox(height: 20),

                // ── KẾ HOẠCH HÔM NAY ──
                Text(
                  'Kế hoạch hôm nay 📋',
                  style: GoogleFonts.outfit(
                    fontSize: 17,
                    fontWeight: FontWeight.bold,
                    color: isDark ? AppColors.darkText : AppColors.lightText,
                  ),
                ),
                const SizedBox(height: 12),

                // Card Thực đơn
                _buildActionCard(
                  isDark: isDark,
                  icon: Icons.restaurant_rounded,
                  iconColor: AppColors.lightCoral,
                  title: 'Thực đơn dinh dưỡng',
                  subtitle: mealDetails.isNotEmpty
                      ? '${mealDetails.length} bữa ăn được gợi ý cho hôm nay'
                      : 'Xem lộ trình bữa ăn 30 ngày chuẩn calo',
                  badge: '${consumedCal > 0 ? consumedCal : tdee} kcal',
                  onTap: () => widget.onNavigateTab(1), // Chuyển sang Tab Dinh dưỡng
                ),
                const SizedBox(height: 12),

                // Card Bài tập
                _buildActionCard(
                  isDark: isDark,
                  icon: Icons.fitness_center_rounded,
                  iconColor: isDark ? AppColors.darkPrimary : AppColors.lightPrimary,
                  title: 'Lịch tập luyện',
                  subtitle: todayWorkout.isNotEmpty
                      ? (todayWorkout['planName'] ?? 'Lộ trình rèn luyện hôm nay')
                      : 'Khám phá danh sách bài tập theo nhóm cơ',
                  badge: 'Chi tiết →',
                  onTap: () => widget.onNavigateTab(2), // Chuyển sang Tab Tập luyện
                ),
                const SizedBox(height: 20),

                // ── THẺ LỜI KHUYÊN AI ──
                _buildAiAdviceCard(isDark: isDark, goal: targetGoal),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildHeroCard({
    required bool isDark,
    required double weight,
    required double heightCm,
    required double bmi,
    required String goalName,
    required int tdee,
    required int consumedCal,
  }) {
    String bmiStatus = 'Bình thường';
    Color bmiColor = AppColors.success;
    if (bmi < 18.5) {
      bmiStatus = 'Hơi gầy';
      bmiColor = AppColors.warning;
    } else if (bmi >= 25) {
      bmiStatus = 'Hơi thừa cân';
      bmiColor = AppColors.error;
    }

    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: isDark ? AppColors.darkCard : AppColors.lightSurface,
        borderRadius: BorderRadius.circular(24),
        border: Border.all(
          color: isDark ? Colors.white.withValues(alpha: 0.08) : Colors.black.withValues(alpha: 0.06),
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: isDark ? 0.2 : 0.04),
            blurRadius: 16,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Mục tiêu
          Row(
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: primary.withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.flag_rounded, size: 14, color: primary),
                    const SizedBox(width: 4),
                    Text(
                      goalName,
                      style: TextStyle(color: primary, fontSize: 12, fontWeight: FontWeight.bold),
                    ),
                  ],
                ),
              ),
              const Spacer(),
              Text(
                'Mục tiêu Calo: $tdee kcal',
                style: TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                  color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                ),
              ),
            ],
          ),
          const SizedBox(height: 18),

          // 3 Chỉ số thể trạng cơ bản
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceAround,
            children: [
              _buildMetricItem(isDark, 'Cân nặng', '${weight.toStringAsFixed(1)} kg', Icons.monitor_weight_outlined),
              Container(width: 1, height: 36, color: isDark ? Colors.white12 : Colors.black12),
              _buildMetricItem(isDark, 'Chiều cao', '${heightCm.round()} cm', Icons.height_rounded),
              Container(width: 1, height: 36, color: isDark ? Colors.white12 : Colors.black12),
              _buildMetricItem(isDark, 'Chỉ số BMI', bmi.toStringAsFixed(1), Icons.analytics_outlined, badge: bmiStatus, badgeColor: bmiColor),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildMetricItem(bool isDark, String label, String value, IconData icon, {String? badge, Color? badgeColor}) {
    return Column(
      children: [
        Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 14, color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
            const SizedBox(width: 4),
            Text(
              label,
              style: TextStyle(fontSize: 11.5, color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
            ),
          ],
        ),
        const SizedBox(height: 4),
        Text(
          value,
          style: GoogleFonts.outfit(
            fontSize: 17,
            fontWeight: FontWeight.bold,
            color: isDark ? AppColors.darkText : AppColors.lightText,
          ),
        ),
        if (badge != null) ...[
          const SizedBox(height: 2),
          Text(
            badge,
            style: TextStyle(fontSize: 10, fontWeight: FontWeight.w600, color: badgeColor ?? AppColors.success),
          ),
        ],
      ],
    );
  }

  Widget _buildActionCard({
    required bool isDark,
    required IconData icon,
    required Color iconColor,
    required String title,
    required String subtitle,
    required String badge,
    required VoidCallback onTap,
  }) {
    return GestureDetector(
      behavior: HitTestBehavior.opaque,
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: isDark ? AppColors.darkCard : AppColors.lightSurface,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: isDark ? Colors.white.withValues(alpha: 0.08) : Colors.black.withValues(alpha: 0.06),
          ),
        ),
        child: Row(
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: iconColor.withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(14),
              ),
              child: Icon(icon, color: iconColor, size: 22),
            ),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 14.5,
                      color: isDark ? AppColors.darkText : AppColors.lightText,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    subtitle,
                    style: TextStyle(
                      fontSize: 12,
                      color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ),
            const SizedBox(width: 8),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
              decoration: BoxDecoration(
                color: (isDark ? AppColors.darkPrimary : AppColors.lightPrimary).withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Text(
                badge,
                style: TextStyle(
                  fontSize: 11.5,
                  fontWeight: FontWeight.bold,
                  color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildAiAdviceCard({required bool isDark, required int goal}) {
    final advice = goal == 1
        ? 'Để giảm cân hiệu quả: Hãy uống 1 cốc nước trước mỗi bữa ăn và ưu tiên rau củ, đạm nạc.'
        : (goal == 2
            ? 'Để duy trì năng lượng: Giữ thói quen ngủ trước 23h và đi bộ nhẹ nhàng 30 phút mỗi ngày.'
            : 'Để tăng cơ tối ưu: Bổ sung đủ protein sau buổi tập và ngủ đủ 7-8 tiếng để cơ bắp phục hồi.');

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: isDark
              ? [AppColors.auroraPurple.withValues(alpha: 0.15), AppColors.auroraCyan.withValues(alpha: 0.1)]
              : [const Color(0xFFEDE9FE), const Color(0xFFE0F2FE)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(
          color: (isDark ? AppColors.auroraPurple : Colors.purple).withValues(alpha: 0.2),
        ),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Icon(Icons.auto_awesome_rounded, color: AppColors.auroraPurple, size: 22),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Lời khuyên thông minh từ AI 💡',
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13.5, color: AppColors.auroraPurple),
                ),
                const SizedBox(height: 4),
                Text(
                  advice,
                  style: TextStyle(
                    fontSize: 12.5,
                    height: 1.4,
                    color: isDark ? AppColors.darkText : AppColors.lightText,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
