import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../core/theme/theme_provider.dart';
import '../../core/theme/app_colors.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../home/dashboard_screen.dart';
import 'forgot_password_screen.dart';
import 'register_screen.dart';

/// Màn hình Đăng nhập — Light: trắng/xanh nhạt, Dark: Navy/Cyan
class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _emailCtrl = TextEditingController();
  final _passwordCtrl = TextEditingController();
  bool _obscure = true;
  bool _isLoading = false;
  String? _error;

  @override
  void dispose() {
    _emailCtrl.dispose();
    _passwordCtrl.dispose();
    super.dispose();
  }

  void _login() async {
    if (_emailCtrl.text.isEmpty || _passwordCtrl.text.isEmpty) {
      setState(() => _error = 'Vui lòng nhập email và mật khẩu.');
      return;
    }
    setState(() { _isLoading = true; _error = null; });

    try {
      await ref.read(authServiceProvider).login(_emailCtrl.text.trim(), _passwordCtrl.text);
      if (mounted) {
        Navigator.pushAndRemoveUntil(
          context,
          PageRouteBuilder(
            transitionDuration: const Duration(milliseconds: 500),
            pageBuilder: (_, __, ___) => const DashboardScreen(),
            transitionsBuilder: (_, anim, __, child) => FadeTransition(opacity: anim, child: child),
          ),
          (_) => false,
        );
      }
    } on DioException catch (e) {
      if (mounted) setState(() => _error = ApiClient.errorMessage(e));
    } catch (e) {
      if (mounted) setState(() => _error = 'Đã xảy ra lỗi, vui lòng thử lại.');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final isDark = ref.watch(themeModeProvider) == ThemeMode.dark;
    final cs = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return Scaffold(
      body: Stack(
        children: [
          // Background
          Positioned.fill(child: isDark ? _darkBg() : _lightBg()),

          SafeArea(
            child: SingleChildScrollView(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 28),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // ── TOP BAR ──
                    Padding(
                      padding: const EdgeInsets.only(top: 8, bottom: 32),
                      child: Row(children: [
                        IconButton(
                          onPressed: () => Navigator.pop(context),
                          icon: Icon(Icons.arrow_back_ios_new_rounded, size: 20, color: cs.onSurface),
                          style: IconButton.styleFrom(
                            backgroundColor: isDark ? AppColors.darkCard : AppColors.lightCard,
                          ),
                        ),
                        const Spacer(),
                        IconButton(
                          onPressed: () => ref.read(themeModeProvider.notifier).toggleTheme(),
                          icon: Icon(isDark ? Icons.light_mode_outlined : Icons.dark_mode_outlined, size: 20),
                          style: IconButton.styleFrom(
                            backgroundColor: isDark ? AppColors.darkCard : AppColors.lightCard,
                            foregroundColor: cs.onSurface,
                          ),
                        ),
                      ]),
                    ),

                    // ── HEADER ──
                    Text('Chào mừng\ntrở lại! 👋',
                      style: textTheme.headlineLarge?.copyWith(
                        fontWeight: FontWeight.w800,
                        height: 1.2,
                        letterSpacing: -0.5,
                        color: isDark ? AppColors.darkText : AppColors.lightText,
                      )),
                    const SizedBox(height: 8),
                    Text('Đăng nhập để tiếp tục lộ trình của bạn.',
                      style: textTheme.bodyLarge?.copyWith(
                        color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                      )),
                    const SizedBox(height: 40),

                    // ── CARD FORM ──
                    Container(
                      padding: const EdgeInsets.all(24),
                      decoration: BoxDecoration(
                        color: isDark ? AppColors.darkCard : Colors.white,
                        borderRadius: BorderRadius.circular(24),
                        boxShadow: isDark
                            ? [BoxShadow(color: AppColors.darkPrimary.withValues(alpha: 0.06), blurRadius: 24, spreadRadius: 0)]
                            : [BoxShadow(color: AppColors.lightPrimary.withValues(alpha: 0.08), blurRadius: 24, offset: const Offset(0, 8))],
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          _label(context, isDark, 'Email'),
                          const SizedBox(height: 8),
                          TextField(
                            controller: _emailCtrl,
                            keyboardType: TextInputType.emailAddress,
                            decoration: InputDecoration(
                              hintText: 'ten@email.com',
                              prefixIcon: Icon(Icons.mail_outline_rounded, color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
                            ),
                          ),
                          const SizedBox(height: 20),
                          _label(context, isDark, 'Mật khẩu'),
                          const SizedBox(height: 8),
                          TextField(
                            controller: _passwordCtrl,
                            obscureText: _obscure,
                            decoration: InputDecoration(
                              hintText: '••••••••',
                              prefixIcon: Icon(Icons.lock_outline_rounded, color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
                              suffixIcon: IconButton(
                                icon: Icon(_obscure ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                                  color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
                                onPressed: () => setState(() => _obscure = !_obscure),
                              ),
                            ),
                          ),

                          // Quên mật khẩu
                          Align(
                            alignment: Alignment.centerRight,
                            child: TextButton(
                              onPressed: () {
                                Navigator.push(
                                  context,
                                  MaterialPageRoute(builder: (_) => const ForgotPasswordScreen()),
                                );
                              },
                              child: Text('Quên mật khẩu?', style: TextStyle(color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary, fontSize: 13)),
                            ),
                          ),

                          // Error
                          if (_error != null) ...[
                            const SizedBox(height: 4),
                            Container(padding: const EdgeInsets.all(12), decoration: BoxDecoration(color: AppColors.error.withValues(alpha: 0.08), borderRadius: BorderRadius.circular(10)), child: Row(children: [const Icon(Icons.error_outline, color: AppColors.error, size: 18), const SizedBox(width: 8), Expanded(child: Text(_error!, style: const TextStyle(color: AppColors.error, fontSize: 13)))])),
                          ],

                          const SizedBox(height: 8),
                          SizedBox(
                            width: double.infinity,
                            child: ElevatedButton(
                              onPressed: _isLoading ? null : _login,
                              style: ElevatedButton.styleFrom(
                                backgroundColor: isDark ? AppColors.darkPrimary : AppColors.lightPrimary,
                                foregroundColor: isDark ? AppColors.darkBg : Colors.white,
                                padding: const EdgeInsets.symmetric(vertical: 16),
                                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                              ),
                              child: _isLoading
                                  ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                                  : const Text('Đăng nhập', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700)),
                            ),
                          ),
                          const SizedBox(height: 16),

                          // Link chuyển sang Đăng ký
                          Center(
                            child: TextButton(
                              onPressed: () => Navigator.push(
                                context,
                                MaterialPageRoute(builder: (_) => const RegisterScreen()),
                              ),
                              child: Text.rich(
                                TextSpan(
                                  text: 'Chưa có tài khoản? ',
                                  style: TextStyle(color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub, fontSize: 13.5),
                                  children: [
                                    TextSpan(
                                      text: 'Đăng ký ngay',
                                      style: TextStyle(
                                        color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary,
                                        fontWeight: FontWeight.bold,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 36),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _label(BuildContext context, bool isDark, String text) {
    return Text(text, style: TextStyle(fontWeight: FontWeight.w700, fontSize: 14, color: isDark ? AppColors.darkText : AppColors.lightText));
  }

  Widget _lightBg() => Container(
    decoration: const BoxDecoration(
      gradient: LinearGradient(begin: Alignment.topCenter, end: Alignment.bottomCenter,
        colors: [Color(0xFFE3F2FD), AppColors.lightBg], stops: [0.0, 0.4]),
    ),
  );

  Widget _darkBg() => Container(
    color: AppColors.darkBg,
    child: Stack(children: [
      Positioned(top: -80, right: -80, child: Container(width: 280, height: 280, decoration: BoxDecoration(shape: BoxShape.circle, color: AppColors.darkPrimary.withValues(alpha: 0.05)))),
    ]),
  );
}
