import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../core/theme/app_colors.dart';
import '../../core/theme/theme_provider.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../home/dashboard_screen.dart';
import 'login_screen.dart';

/// Màn hình Đăng ký tinh gọn cho Đồ án:
/// Gom toàn bộ luồng khảo sát 15 bước thành 1 màn hình duy nhất
/// gồm thông tin tài khoản và chỉ số thể trạng cơ bản.
class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();

  // Thông tin tài khoản
  final _fullNameCtrl = TextEditingController();
  final _emailCtrl = TextEditingController();
  final _passwordCtrl = TextEditingController();
  final _confirmPasswordCtrl = TextEditingController();

  // Thông số thể trạng
  final _heightCtrl = TextEditingController(text: '170');
  final _weightCtrl = TextEditingController(text: '65');
  final _ageCtrl = TextEditingController(text: '22');

  int _selectedGender = 0; // 0: Nam, 1: Nữ
  int _selectedGoal = 1;   // 1: Giảm cân, 2: Duy trì, 3: Tăng cơ

  bool _obscurePassword = true;
  bool _obscureConfirm = true;
  bool _isLoading = false;
  String? _errorMessage;

  @override
  void dispose() {
    _fullNameCtrl.dispose();
    _emailCtrl.dispose();
    _passwordCtrl.dispose();
    _confirmPasswordCtrl.dispose();
    _heightCtrl.dispose();
    _weightCtrl.dispose();
    _ageCtrl.dispose();
    super.dispose();
  }

  Future<void> _handleRegister() async {
    if (!_formKey.currentState!.validate()) return;

    if (_passwordCtrl.text != _confirmPasswordCtrl.text) {
      setState(() => _errorMessage = 'Mật khẩu xác nhận không khớp.');
      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    final age = int.tryParse(_ageCtrl.text) ?? 22;
    final birthYear = DateTime.now().year - age;
    final dob = DateTime(birthYear, 1, 1);

    final payload = {
      'email': _emailCtrl.text.trim(),
      'password': _passwordCtrl.text,
      'confirmPassword': _confirmPasswordCtrl.text,
      'fullName': _fullNameCtrl.text.trim(),
      'gender': _selectedGender,
      'heightCm': double.tryParse(_heightCtrl.text) ?? 170.0,
      'weight': double.tryParse(_weightCtrl.text) ?? 65.0,
      'dateOfBirth': dob.toIso8601String(),
      'targetGoal': _selectedGoal,
      'fitnessLevel': 2,
    };

    try {
      await ref.read(authServiceProvider).registerWithOnboarding(payload);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('🎉 Đăng ký thành công! Chào mừng bạn đến với AnFit.'),
            backgroundColor: AppColors.success,
          ),
        );

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
      if (mounted) setState(() => _errorMessage = ApiClient.errorMessage(e));
    } catch (e) {
      if (mounted) setState(() => _errorMessage = 'Đã xảy ra lỗi, vui lòng thử lại.');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final isDark = ref.watch(themeModeProvider) == ThemeMode.dark;
    final cs = Theme.of(context).colorScheme;

    return Scaffold(
      backgroundColor: isDark ? AppColors.darkBg : AppColors.lightBg,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Top Bar
                Row(
                  children: [
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
                  ],
                ),
                const SizedBox(height: 20),

                // Title
                Text(
                  'Tạo tài khoản 🚀',
                  style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                    fontWeight: FontWeight.w800,
                    color: isDark ? AppColors.darkText : AppColors.lightText,
                  ),
                ),
                const SizedBox(height: 6),
                Text(
                  'Thiết lập thông tin để AnFit xây dựng lộ trình luyện tập & dinh dưỡng cho bạn.',
                  style: TextStyle(
                    color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                    fontSize: 14,
                    height: 1.4,
                  ),
                ),
                const SizedBox(height: 24),

                // Error Banner
                if (_errorMessage != null) ...[
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: AppColors.error.withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: AppColors.error.withValues(alpha: 0.3)),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.error_outline, color: AppColors.error, size: 20),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(_errorMessage!, style: const TextStyle(color: AppColors.error, fontSize: 13)),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 16),
                ],

                // ── PHẦN 1: TÀI KHOẢN ──
                _buildSectionCard(
                  isDark: isDark,
                  title: 'Thông tin tài khoản',
                  icon: Icons.person_outline,
                  children: [
                    _buildTextField(
                      controller: _fullNameCtrl,
                      label: 'Họ và tên',
                      hint: 'Nguyễn Văn A',
                      icon: Icons.badge_outlined,
                      isDark: isDark,
                      validator: (v) => (v == null || v.trim().isEmpty) ? 'Vui lòng nhập họ tên' : null,
                    ),
                    const SizedBox(height: 14),
                    _buildTextField(
                      controller: _emailCtrl,
                      label: 'Email',
                      hint: 'email@example.com',
                      icon: Icons.mail_outline,
                      keyboardType: TextInputType.emailAddress,
                      isDark: isDark,
                      validator: (v) {
                        if (v == null || v.trim().isEmpty) return 'Vui lòng nhập email';
                        if (!v.contains('@') || !v.contains('.')) return 'Email không hợp lệ';
                        return null;
                      },
                    ),
                    const SizedBox(height: 14),
                    _buildTextField(
                      controller: _passwordCtrl,
                      label: 'Mật khẩu',
                      hint: 'Tối thiểu 8 ký tự',
                      icon: Icons.lock_outline,
                      obscureText: _obscurePassword,
                      isDark: isDark,
                      suffixIcon: IconButton(
                        icon: Icon(_obscurePassword ? Icons.visibility_off : Icons.visibility, size: 20),
                        onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
                      ),
                      validator: (v) => (v == null || v.length < 8) ? 'Mật khẩu phải từ 8 ký tự' : null,
                    ),
                    const SizedBox(height: 14),
                    _buildTextField(
                      controller: _confirmPasswordCtrl,
                      label: 'Xác nhận mật khẩu',
                      hint: 'Nhập lại mật khẩu',
                      icon: Icons.lock_reset,
                      obscureText: _obscureConfirm,
                      isDark: isDark,
                      suffixIcon: IconButton(
                        icon: Icon(_obscureConfirm ? Icons.visibility_off : Icons.visibility, size: 20),
                        onPressed: () => setState(() => _obscureConfirm = !_obscureConfirm),
                      ),
                      validator: (v) => (v == null || v.isEmpty) ? 'Vui lòng xác nhận mật khẩu' : null,
                    ),
                  ],
                ),
                const SizedBox(height: 20),

                // ── PHẦN 2: THỂ TRẠNG & MỤC TIÊU ──
                _buildSectionCard(
                  isDark: isDark,
                  title: 'Thể trạng & Mục tiêu',
                  icon: Icons.fitness_center_outlined,
                  children: [
                    // Giới tính
                    const Text('Giới tính', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        Expanded(
                          child: _buildChoiceChip(
                            label: 'Nam 👦',
                            selected: _selectedGender == 0,
                            onTap: () => setState(() => _selectedGender = 0),
                            isDark: isDark,
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: _buildChoiceChip(
                            label: 'Nữ 👧',
                            selected: _selectedGender == 1,
                            onTap: () => setState(() => _selectedGender = 1),
                            isDark: isDark,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),

                    // Chiều cao & Cân nặng & Tuổi
                    Row(
                      children: [
                        Expanded(
                          child: _buildTextField(
                            controller: _heightCtrl,
                            label: 'Chiều cao (cm)',
                            hint: '170',
                            keyboardType: TextInputType.number,
                            isDark: isDark,
                            validator: (v) => (double.tryParse(v ?? '') == null) ? 'Lỗi' : null,
                          ),
                        ),
                        const SizedBox(width: 10),
                        Expanded(
                          child: _buildTextField(
                            controller: _weightCtrl,
                            label: 'Cân nặng (kg)',
                            hint: '65',
                            keyboardType: TextInputType.number,
                            isDark: isDark,
                            validator: (v) => (double.tryParse(v ?? '') == null) ? 'Lỗi' : null,
                          ),
                        ),
                        const SizedBox(width: 10),
                        Expanded(
                          child: _buildTextField(
                            controller: _ageCtrl,
                            label: 'Tuổi',
                            hint: '22',
                            keyboardType: TextInputType.number,
                            isDark: isDark,
                            validator: (v) => (int.tryParse(v ?? '') == null) ? 'Lỗi' : null,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),

                    // Mục tiêu
                    const Text('Mục tiêu chính', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                    const SizedBox(height: 8),
                    _buildGoalOption(
                      title: 'Giảm cân & Đốt mỡ 🔥',
                      subtitle: 'Thực đơn thâm hụt calo nhẹ và bài tập cardio/HIIT',
                      goalValue: 1,
                      isDark: isDark,
                    ),
                    const SizedBox(height: 8),
                    _buildGoalOption(
                      title: 'Duy trì vóc dáng ⚖️',
                      subtitle: 'Giữ cân nặng ổn định và nâng cao sức bền hàng ngày',
                      goalValue: 2,
                      isDark: isDark,
                    ),
                    const SizedBox(height: 8),
                    _buildGoalOption(
                      title: 'Tăng cơ & Săn chắc 💪',
                      subtitle: 'Giàu đạm (protein) và bài tập kháng lực phát triển cơ',
                      goalValue: 3,
                      isDark: isDark,
                    ),
                  ],
                ),
                const SizedBox(height: 28),

                // Nút Đăng ký
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: _isLoading ? null : _handleRegister,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: isDark ? AppColors.darkPrimary : AppColors.lightPrimary,
                      foregroundColor: isDark ? AppColors.darkBg : Colors.white,
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                      elevation: isDark ? 0 : 2,
                    ),
                    child: _isLoading
                        ? const SizedBox(
                            width: 22, height: 22,
                            child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                          )
                        : const Text(
                            'Hoàn tất & Bắt đầu ngay 🚀',
                            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                          ),
                  ),
                ),
                const SizedBox(height: 16),

                // Link chuyển sang Login
                Center(
                  child: TextButton(
                    onPressed: () => Navigator.pushReplacement(
                      context,
                      MaterialPageRoute(builder: (_) => const LoginScreen()),
                    ),
                    child: Text.rich(
                      TextSpan(
                        text: 'Đã có tài khoản? ',
                        style: TextStyle(color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
                        children: [
                          TextSpan(
                            text: 'Đăng nhập ngay',
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
                const SizedBox(height: 20),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildSectionCard({
    required bool isDark,
    required String title,
    required IconData icon,
    required List<Widget> children,
  }) {
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: isDark ? AppColors.darkCard : AppColors.lightSurface,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(
          color: isDark ? Colors.white.withValues(alpha: 0.08) : Colors.black.withValues(alpha: 0.06),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, size: 20, color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary),
              const SizedBox(width: 8),
              Text(
                title,
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 15,
                  color: isDark ? AppColors.darkText : AppColors.lightText,
                ),
              ),
            ],
          ),
          const Divider(height: 24),
          ...children,
        ],
      ),
    );
  }

  Widget _buildTextField({
    required TextEditingController controller,
    required String label,
    required String hint,
    required bool isDark,
    IconData? icon,
    TextInputType? keyboardType,
    bool obscureText = false,
    Widget? suffixIcon,
    String? Function(String?)? validator,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: TextStyle(
            fontWeight: FontWeight.w600,
            fontSize: 12.5,
            color: isDark ? AppColors.darkText : AppColors.lightText,
          ),
        ),
        const SizedBox(height: 6),
        TextFormField(
          controller: controller,
          keyboardType: keyboardType,
          obscureText: obscureText,
          validator: validator,
          style: TextStyle(color: isDark ? AppColors.darkText : AppColors.lightText, fontSize: 14),
          decoration: InputDecoration(
            hintText: hint,
            hintStyle: TextStyle(
              color: isDark ? AppColors.darkTextSub.withValues(alpha: 0.6) : AppColors.lightTextSub.withValues(alpha: 0.6),
              fontSize: 13,
            ),
            prefixIcon: icon != null ? Icon(icon, size: 18) : null,
            suffixIcon: suffixIcon,
            filled: true,
            fillColor: isDark ? AppColors.darkBgAlt : AppColors.lightCard,
            contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
            border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide.none),
            enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide.none),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary, width: 1.5),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildChoiceChip({
    required String label,
    required bool selected,
    required VoidCallback onTap,
    required bool isDark,
  }) {
    final activeColor = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 12),
        alignment: Alignment.center,
        decoration: BoxDecoration(
          color: selected
              ? activeColor.withValues(alpha: 0.15)
              : (isDark ? AppColors.darkBgAlt : AppColors.lightCard),
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: selected ? activeColor : Colors.transparent,
            width: 1.5,
          ),
        ),
        child: Text(
          label,
          style: TextStyle(
            fontWeight: selected ? FontWeight.bold : FontWeight.normal,
            color: selected ? activeColor : (isDark ? AppColors.darkText : AppColors.lightText),
          ),
        ),
      ),
    );
  }

  Widget _buildGoalOption({
    required String title,
    required String subtitle,
    required int goalValue,
    required bool isDark,
  }) {
    final selected = _selectedGoal == goalValue;
    final activeColor = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;

    return GestureDetector(
      onTap: () => setState(() => _selectedGoal = goalValue),
      child: Container(
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: selected
              ? activeColor.withValues(alpha: 0.1)
              : (isDark ? AppColors.darkBgAlt : AppColors.lightCard),
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: selected ? activeColor : Colors.transparent,
            width: 1.5,
          ),
        ),
        child: Row(
          children: [
            Icon(
              selected ? Icons.radio_button_checked : Icons.radio_button_off,
              color: selected ? activeColor : (isDark ? AppColors.darkTextSub : AppColors.lightTextSub),
              size: 20,
            ),
            const SizedBox(width: 10),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 13.5,
                      color: selected ? activeColor : (isDark ? AppColors.darkText : AppColors.lightText),
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    subtitle,
                    style: TextStyle(
                      fontSize: 11.5,
                      color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
