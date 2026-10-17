import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/theme_provider.dart';
import '../../core/theme/app_colors.dart';
import 'login_screen.dart';
import 'register_screen.dart';

/// Màn hình chào mừng — Frictionless: chỉ 1 nút "Bắt đầu ngay"
class EntryScreen extends ConsumerWidget {
  const EntryScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final isDark = ref.watch(themeModeProvider) == ThemeMode.dark;
    final cs = Theme.of(context).colorScheme;

    return Scaffold(
      body: Stack(
        children: [
          // ── BACKGROUND ──
          Positioned.fill(
            child: isDark
                ? _darkBackground()
                : _lightBackground(),
          ),

          // ── THEME TOGGLE ──
          Positioned(
            top: 56, right: 20,
            child: IconButton(
              onPressed: () => ref.read(themeModeProvider.notifier).toggleTheme(),
              icon: Icon(isDark ? Icons.light_mode_outlined : Icons.dark_mode_outlined, size: 22),
              style: IconButton.styleFrom(
                backgroundColor: isDark ? AppColors.darkCard : AppColors.lightCard,
                foregroundColor: cs.onSurface,
              ),
            ),
          ),

          // ── MAIN CONTENT ──
          SafeArea(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 28),
              child: Column(
                children: [
                  const Spacer(flex: 2),

                  // Logo / Brand
                  _buildLogo(isDark),

                  const Spacer(flex: 1),

                  // Headline
                  Text(
                    'Sức khỏe tốt hơn\nbắt đầu từ hôm nay.',
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.w800,
                      height: 1.25,
                      color: isDark ? AppColors.darkText : AppColors.lightText,
                      letterSpacing: -0.5,
                    ),
                  ),
                  const SizedBox(height: 12),
                  Text(
                    'AI tạo lộ trình tập luyện & dinh dưỡng\ncá nhân hóa chỉ trong 2 phút.',
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                      color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                      height: 1.5,
                    ),
                  ),

                  const Spacer(flex: 2),

                  // Feature pills
                  _buildFeaturePills(context, isDark),

                  const Spacer(flex: 3),

                  // ── NÚT CHÍNH: BẮT ĐẦU NGAY ──
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: () => Navigator.push(
                        context,
                        MaterialPageRoute(builder: (_) => const RegisterScreen()),
                      ),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: isDark ? AppColors.darkPrimary : AppColors.lightPrimary,
                        foregroundColor: isDark ? AppColors.darkBg : Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 18),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(18)),
                        elevation: isDark ? 0 : 2,
                      ),
                      child: const Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text('Bắt đầu ngay', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w700)),
                          SizedBox(width: 8),
                          Icon(Icons.arrow_forward_rounded, size: 20),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // ── LINK NHỎ: ĐÃ CÓ TÀI KHOẢN ──
                  TextButton(
                    onPressed: () => Navigator.push(
                      context,
                      MaterialPageRoute(builder: (_) => const LoginScreen()),
                    ),
                    child: Text(
                      'Tôi đã có tài khoản  →',
                      style: TextStyle(
                        color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                        fontSize: 14,
                        decoration: TextDecoration.underline,
                        decorationColor: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                      ),
                    ),
                  ),
                  const SizedBox(height: 24),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLogo(bool isDark) {
    return Column(children: [
      Container(
        width: 80, height: 80,
        decoration: BoxDecoration(
          color: isDark ? AppColors.darkPrimary.withValues(alpha: 0.15) : AppColors.lightPrimary.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(24),
          border: Border.all(color: isDark ? AppColors.darkPrimary.withValues(alpha: 0.3) : AppColors.lightPrimary.withValues(alpha: 0.25), width: 1.5),
        ),
        child: Icon(Icons.flash_on_rounded, size: 44, color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary),
      ),
      const SizedBox(height: 14),
      Text('AnFit', style: TextStyle(fontSize: 32, fontWeight: FontWeight.w900, letterSpacing: -1, color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary)),
      Text('Hybrid AI · Personal Fitness', style: TextStyle(fontSize: 12, letterSpacing: 1.5, color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub)),
    ]);
  }

  Widget _buildFeaturePills(BuildContext context, bool isDark) {
    final items = [
      (Icons.auto_awesome, 'AI Cá nhân hóa'),
      (Icons.restaurant_outlined, 'Thực đơn dinh dưỡng'),
      (Icons.fitness_center_outlined, 'Lộ trình luyện tập'),
    ];
    return Wrap(
      spacing: 8, runSpacing: 8, alignment: WrapAlignment.center,
      children: items.map((item) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
        decoration: BoxDecoration(
          color: isDark ? AppColors.darkCard : AppColors.lightCard,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: isDark ? AppColors.darkPrimary.withValues(alpha: 0.2) : AppColors.lightPrimary.withValues(alpha: 0.15)),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          Icon(item.$1, size: 15, color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary),
          const SizedBox(width: 6),
          Text(item.$2, style: TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: isDark ? AppColors.darkText : AppColors.lightText)),
        ]),
      )).toList(),
    );
  }

  Widget _darkBackground() {
    return Container(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [AppColors.darkBg, Color(0xFF0D1F35), AppColors.darkBg],
          stops: [0.0, 0.5, 1.0],
        ),
      ),
      child: Stack(children: [
        Positioned(top: -60, right: -60, child: Container(width: 220, height: 220, decoration: BoxDecoration(shape: BoxShape.circle, color: AppColors.darkPrimary.withValues(alpha: 0.06)))),
        Positioned(bottom: -80, left: -80, child: Container(width: 280, height: 280, decoration: BoxDecoration(shape: BoxShape.circle, color: AppColors.darkPrimaryGlow.withValues(alpha: 0.05)))),
      ]),
    );
  }

  Widget _lightBackground() {
    return Container(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [Color(0xFFE3F2FD), AppColors.lightBg],
          stops: [0.0, 0.5],
        ),
      ),
      child: Stack(children: [
        Positioned(top: -40, right: -40, child: Container(width: 200, height: 200, decoration: BoxDecoration(shape: BoxShape.circle, color: AppColors.lightPrimary.withValues(alpha: 0.06)))),
        Positioned(bottom: -60, left: -60, child: Container(width: 250, height: 250, decoration: BoxDecoration(shape: BoxShape.circle, color: AppColors.lightPrimary.withValues(alpha: 0.04)))),
      ]),
    );
  }
}
