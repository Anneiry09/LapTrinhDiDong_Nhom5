import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/theme_provider.dart';
import '../../core/theme/app_colors.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../../theme/anfit_theme.dart'; // Sử dụng GlassmorphismCard hoặc AiGlowingButton

/// Màn hình Khôi phục Mật khẩu — 2026 Premium Wellness Style
/// Quy trình 2 bước mượt mà (Frictionless OTP reset flow):
/// - Bước 1: Nhập email để nhận mã OTP 6 ký tự
/// - Bước 2: Nhập OTP và thiết lập mật khẩu mới
class ForgotPasswordScreen extends ConsumerStatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  ConsumerState<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends ConsumerState<ForgotPasswordScreen> {
  final _emailCtrl = TextEditingController();
  final _otpCtrl = TextEditingController();
  final _newPasswordCtrl = TextEditingController();
  final _confirmPasswordCtrl = TextEditingController();

  bool _isOtpSent = false;
  bool _isLoading = false;
  bool _obscureNew = true;
  bool _obscureConfirm = true;
  String? _error;
  String? _successMessage;

  @override
  void dispose() {
    _emailCtrl.dispose();
    _otpCtrl.dispose();
    _newPasswordCtrl.dispose();
    _confirmPasswordCtrl.dispose();
    super.dispose();
  }

  /// Bước 1: Gửi yêu cầu lấy mã OTP
  void _requestOtp() async {
    final email = _emailCtrl.text.trim();
    if (email.isEmpty) {
      setState(() => _error = 'Vui lòng nhập địa chỉ email.');
      return;
    }

    setState(() {
      _isLoading = true;
      _error = null;
      _successMessage = null;
    });

    try {
      final result = await ref.read(authServiceProvider).forgotPassword(email);
      
      setState(() {
        _isOtpSent = true;
        _successMessage = result['message'] ?? 'Mã khôi phục đã được gửi. Vui lòng kiểm tra hộp thư.';
      });
    } on DioException catch (e) {
      setState(() => _error = ApiClient.errorMessage(e));
    } catch (e) {
      setState(() => _error = 'Đã xảy ra lỗi, vui lòng thử lại.');
    } finally {
      setState(() => _isLoading = false);
    }
  }

  /// Bước 2: Thực hiện đặt lại mật khẩu với mã OTP
  void _resetPassword() async {
    final otp = _otpCtrl.text.trim();
    final newPass = _newPasswordCtrl.text;
    final confirmPass = _confirmPasswordCtrl.text;

    if (otp.isEmpty || newPass.isEmpty || confirmPass.isEmpty) {
      setState(() => _error = 'Vui lòng nhập đầy đủ mã OTP và mật khẩu mới.');
      return;
    }

    if (newPass != confirmPass) {
      setState(() => _error = 'Xác nhận mật khẩu mới không khớp.');
      return;
    }

    if (newPass.length < 6) {
      setState(() => _error = 'Mật khẩu phải chứa ít nhất 6 ký tự.');
      return;
    }

    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      await ref.read(authServiceProvider).resetPassword(
        token: otp,
        newPassword: newPass,
        confirmPassword: confirmPass,
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('✅ Đặt lại mật khẩu thành công! Vui lòng đăng nhập lại.'),
            backgroundColor: AppColors.success,
          ),
        );
        Navigator.pop(context); // Quay về màn hình Login
      }
    } on DioException catch (e) {
      setState(() => _error = ApiClient.errorMessage(e));
    } catch (e) {
      setState(() => _error = 'Đã xảy ra lỗi, vui lòng thử lại.');
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
      backgroundColor: isDark ? AppColors.darkBg : AppColors.lightBg,
      body: Stack(
        children: [
          // Background Gradient nhã nhặn
          Positioned.fill(
            child: isDark ? _darkBg() : _lightBg(),
          ),

          SafeArea(
            child: SingleChildScrollView(
              physics: const BouncingScrollPhysics(),
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 28),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // ── TOP NAVIGATION ──
                    Padding(
                      padding: const EdgeInsets.only(top: 8, bottom: 24),
                      child: Row(
                        children: [
                          IconButton(
                            onPressed: () => Navigator.pop(context),
                            icon: Icon(Icons.arrow_back_ios_new_rounded, size: 20, color: cs.onSurface),
                            style: IconButton.styleFrom(
                              backgroundColor: isDark ? AppColors.darkCard : AppColors.lightCard,
                            ),
                          ),
                          const Spacer(),
                          Text(
                            'Khôi phục mật khẩu',
                            style: GoogleFonts.outfit(
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                              color: isDark ? AppColors.darkText : AppColors.lightText,
                            ),
                          ),
                          const Spacer(),
                          const SizedBox(width: 40), // Cân bằng với nút Back
                        ],
                      ),
                    ),

                    // ── HEADER ──
                    Text(
                      _isOtpSent ? 'Thiết lập\nmật khẩu mới 🔐' : 'Quên\nmật khẩu? 💡',
                      style: GoogleFonts.outfit(
                        fontSize: 32,
                        fontWeight: FontWeight.w800,
                        height: 1.2,
                        letterSpacing: -0.5,
                        color: isDark ? AppColors.darkText : AppColors.lightText,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      _isOtpSent
                          ? 'Vui lòng điền mã OTP và mật khẩu bảo mật mới của bạn.'
                          : 'Nhập email liên kết với tài khoản để nhận mã OTP đặt lại mật khẩu.',
                      style: textTheme.bodyLarge?.copyWith(
                        color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                        fontSize: 15,
                        height: 1.4,
                      ),
                    ),
                    const SizedBox(height: 32),

                    // ── DÙNG GLASSMORPHISM CARD CHUẨN 2026 PREMIUM WELLNESS ──
                    GlassmorphismCard(
                      padding: const EdgeInsets.all(24),
                      borderRadius: 24,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // TRẠNG THÁI 1: NHẬP EMAIL
                          if (!_isOtpSent) ...[
                            _label(context, isDark, 'Địa chỉ Email'),
                            const SizedBox(height: 8),
                            TextField(
                              controller: _emailCtrl,
                              keyboardType: TextInputType.emailAddress,
                              style: TextStyle(color: isDark ? AppColors.darkText : AppColors.lightText),
                              decoration: InputDecoration(
                                hintText: 'ten@email.com',
                                prefixIcon: Icon(Icons.mail_outline_rounded,
                                    color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
                              ),
                            ),
                            const SizedBox(height: 24),
                            SizedBox(
                              width: double.infinity,
                              child: _isLoading
                                  ? const Center(child: CircularProgressIndicator(color: AnFitTheme.primaryAccent))
                                  : AiGlowingButton(
                                      text: 'Gửi mã xác nhận',
                                      icon: Icons.send_rounded,
                                      onPressed: _requestOtp,
                                    ),
                            ),
                          ],

                          // TRẠNG THÁI 2: NHẬP OTP & MẬT KHẨU MỚI
                          if (_isOtpSent) ...[
                            _label(context, isDark, 'Mã xác nhận (OTP)'),
                            const SizedBox(height: 8),
                            TextField(
                              controller: _otpCtrl,
                              keyboardType: TextInputType.text,
                              textCapitalization: TextCapitalization.characters,
                              maxLength: 6,
                              style: GoogleFonts.outfit(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                letterSpacing: 6,
                                color: isDark ? AppColors.darkPrimaryGlow : AppColors.lightPrimary,
                              ),
                              decoration: InputDecoration(
                                hintText: 'A1B2C3',
                                counterText: '',
                                prefixIcon: Icon(Icons.password_rounded,
                                    color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
                              ),
                            ),
                            const SizedBox(height: 16),
                            _label(context, isDark, 'Mật khẩu mới'),
                            const SizedBox(height: 8),
                            TextField(
                              controller: _newPasswordCtrl,
                              obscureText: _obscureNew,
                              style: TextStyle(color: isDark ? AppColors.darkText : AppColors.lightText),
                              decoration: InputDecoration(
                                hintText: 'Tối thiểu 6 ký tự',
                                prefixIcon: Icon(Icons.lock_outline_rounded,
                                    color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
                                suffixIcon: IconButton(
                                  icon: Icon(
                                      _obscureNew ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                                      color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
                                  onPressed: () => setState(() => _obscureNew = !_obscureNew),
                                ),
                              ),
                            ),
                            const SizedBox(height: 16),
                            _label(context, isDark, 'Xác nhận mật khẩu mới'),
                            const SizedBox(height: 8),
                            TextField(
                              controller: _confirmPasswordCtrl,
                              obscureText: _obscureConfirm,
                              style: TextStyle(color: isDark ? AppColors.darkText : AppColors.lightText),
                              decoration: InputDecoration(
                                hintText: 'Nhập lại mật khẩu mới',
                                prefixIcon: Icon(Icons.lock_outline_rounded,
                                    color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
                                suffixIcon: IconButton(
                                  icon: Icon(
                                      _obscureConfirm ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                                      color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
                                  onPressed: () => setState(() => _obscureConfirm = !_obscureConfirm),
                                ),
                              ),
                            ),
                            const SizedBox(height: 24),
                            SizedBox(
                              width: double.infinity,
                              child: _isLoading
                                  ? const Center(child: CircularProgressIndicator(color: AnFitTheme.primaryAccent))
                                  : AiGlowingButton(
                                      text: 'Đặt lại mật khẩu',
                                      icon: Icons.vpn_key_rounded,
                                      onPressed: _resetPassword,
                                    ),
                            ),
                          ],

                          // THÔNG BÁO LỖI (ERROR STATUS)
                          if (_error != null) ...[
                            const SizedBox(height: 16),
                            Container(
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: AppColors.error.withValues(alpha: 0.08),
                                borderRadius: BorderRadius.circular(14),
                                border: Border.all(color: AppColors.error.withValues(alpha: 0.2), width: 0.5),
                              ),
                              child: Row(
                                children: [
                                  const Icon(Icons.error_outline_rounded, color: AppColors.error, size: 20),
                                  const SizedBox(width: 10),
                                  Expanded(
                                    child: Text(
                                      _error!,
                                      style: GoogleFonts.inter(color: AppColors.error, fontSize: 13, height: 1.3),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],

                          // THÔNG BÁO THÀNH CÔNG / DEBUG OTP TOOLTIP (FRICTIONLESS WELLNESS)
                          if (_successMessage != null) ...[
                            const SizedBox(height: 16),
                            Container(
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: isDark
                                    ? AppColors.darkPrimary.withValues(alpha: 0.08)
                                    : AppColors.lightPrimary.withValues(alpha: 0.08),
                                borderRadius: BorderRadius.circular(14),
                                border: Border.all(
                                    color: isDark
                                        ? AppColors.darkPrimary.withValues(alpha: 0.2)
                                        : AppColors.lightPrimary.withValues(alpha: 0.2),
                                    width: 0.5),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    children: [
                                      Icon(
                                        Icons.check_circle_outline_rounded,
                                        color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary,
                                        size: 20,
                                      ),
                                      const SizedBox(width: 10),
                                      Expanded(
                                        child: Text(
                                          _successMessage!,
                                          style: GoogleFonts.inter(
                                            color: isDark ? AppColors.darkText : AppColors.lightText,
                                            fontSize: 13,
                                            fontWeight: FontWeight.w500,
                                            height: 1.3,
                                          ),
                                        ),
                                      ),
                                    ],
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ],
                      ),
                    ),
                    const SizedBox(height: 40),
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
    return Text(
      text,
      style: GoogleFonts.inter(
        fontWeight: FontWeight.w700,
        fontSize: 13,
        color: isDark ? AppColors.darkText : AppColors.lightText,
      ),
    );
  }

  Widget _lightBg() => Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [Color(0xFFE3F2FD), AppColors.lightBg],
            stops: [0.0, 0.4],
          ),
        ),
      );

  Widget _darkBg() => Container(
        color: AppColors.darkBg,
        child: Stack(
          children: [
            Positioned(
              top: -80,
              right: -80,
              child: Container(
                width: 280,
                height: 280,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: AppColors.darkPrimary.withValues(alpha: 0.05),
                ),
              ),
            ),
          ],
        ),
      );
}
