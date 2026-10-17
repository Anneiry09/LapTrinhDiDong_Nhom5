import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

/// ============================================================================
/// TOKENS HỆ THỐNG THIẾT KẾ: 2026 Premium Wellness & Spatial Design
/// ============================================================================
class AnFitTheme {
  // 1. Color Palette (Bảng màu Premium Wellness)
  static const Color scaffoldBackground = Color(0xFF12171C); // Deep Charcoal nhã nhặn, không đen thuần
  static const Color surfaceGlass = Color(0xFF1E2630);       // Nền phụ của các thẻ kính mờ
  
  static const Color primaryAccent = Color(0xFF10B981);      // Soft Emerald dịu mắt
  static const Color primaryMint = Color(0xFF34D399);        // Mint Glow phát sáng nhẹ
  
  // Aurora Mesh Gradient (Màu đặc trưng của tính năng AI)
  static const Color auroraPurple = Color(0xFFA78BFA);      // Pastel Purple
  static const Color auroraCyan = Color(0xFF67E8F9);        // Soft Cyan
  static const Color auroraPearl = Color(0xFFF5F5F7);       // Pearl White
  
  static const Color textPrimary = Color(0xFFF3F4F6);        // Xám siêu nhạt cho text chính
  static const Color textSecondary = Color(0xFF9CA3AF);      // Xám nhạt cho text phụ
  
  // Gradient màu AI hoàn chỉnh (Sử dụng static final thay vì const để tránh lỗi khởi tạo Dart)
  static final Gradient auroraGradient = LinearGradient(
    colors: const [auroraPurple, auroraCyan, auroraPearl],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  /// Định nghĩa ThemeData cho toàn bộ ứng dụng AnFit
  static ThemeData get darkTheme {
    return ThemeData(
      brightness: Brightness.dark,
      scaffoldBackgroundColor: scaffoldBackground,
      primaryColor: primaryAccent,
      // Cấu hình ColorScheme chuẩn Flutter 2026 (Loại bỏ thuộc tính background cũ đã bị deprecated)
      colorScheme: const ColorScheme.dark(
        primary: primaryAccent,
        secondary: primaryMint,
        surface: surfaceGlass,
        onSurface: textPrimary,
      ),
      textTheme: TextTheme(
        headlineLarge: GoogleFonts.outfit(
          fontSize: 32,
          fontWeight: FontWeight.bold,
          color: textPrimary,
          letterSpacing: -0.5,
        ),
        headlineMedium: GoogleFonts.outfit(
          fontSize: 24,
          fontWeight: FontWeight.w600,
          color: textPrimary,
        ),
        titleLarge: GoogleFonts.outfit(
          fontSize: 20,
          fontWeight: FontWeight.w600,
          color: textPrimary,
        ),
        bodyLarge: GoogleFonts.inter(
          fontSize: 16,
          fontWeight: FontWeight.normal,
          color: textPrimary,
        ),
        bodyMedium: GoogleFonts.inter(
          fontSize: 14,
          fontWeight: FontWeight.normal,
          color: textSecondary,
        ),
      ),
    );
  }
}

/// ============================================================================
/// REUSABLE WIDGET 1: GlassmorphismCard (Thẻ Kính Mờ Cao Cấp)
/// ============================================================================
class GlassmorphismCard extends StatelessWidget {
  final Widget child;
  final double borderRadius;
  final double blurSigma;
  final EdgeInsetsGeometry? padding;
  final EdgeInsetsGeometry? margin;
  final double? width;
  final double? height;

  const GlassmorphismCard({
    super.key,                // Sử dụng super parameters theo chuẩn mới của Flutter
    required this.child,
    this.borderRadius = 24.0, // Bo góc lớn theo đúng chuẩn squircle 2026
    this.blurSigma = 12.0,    // Sigma mờ từ 10 đến 15
    this.padding,
    this.margin,
    this.width,
    this.height,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: width,
      height: height,
      margin: margin,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(borderRadius),
        boxShadow: [
          // Bóng mờ nhẹ phía sau tạo chiều sâu không gian (Spatial Depth)
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.15), // Sử dụng withValues chuẩn Flutter 2026
            blurRadius: 24,
            spreadRadius: -4,
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(borderRadius),
        child: BackdropFilter(
          filter: ImageFilter.blur(sigmaX: blurSigma, sigmaY: blurSigma),
          child: Container(
            padding: padding ?? const EdgeInsets.all(20),
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(borderRadius),
              // Phủ 6% màu trắng mờ để làm nền phản chiếu nhẹ ánh sáng tinh tế
              color: Colors.white.withValues(alpha: 0.06),
              // Viền kính siêu mỏng 0.5px
              border: Border.all(
                color: Colors.white.withValues(alpha: 0.12),
                width: 0.5,
              ),
            ),
            child: child,
          ),
        ),
      ),
    );
  }
}

/// ============================================================================
/// REUSABLE WIDGET 2: AiGlowingButton (Nút Bấm AI Phát Sáng Mềm Mại)
/// ============================================================================
class AiGlowingButton extends StatelessWidget {
  final String text;
  final VoidCallback onPressed;
  final IconData? icon;
  final double height;
  final double borderRadius;

  const AiGlowingButton({
    super.key,                // Sử dụng super parameters theo chuẩn mới của Flutter
    required this.text,
    required this.onPressed,
    this.icon,
    this.height = 56.0,
    this.borderRadius = 28.0, // Bo góc cong hoàn toàn mềm mại
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      height: height,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(borderRadius),
        // Hiệu ứng bóng phát sáng màu dịu (Soft Colored Glow) sử dụng withValues chuẩn 2026
        boxShadow: [
          BoxShadow(
            color: AnFitTheme.auroraCyan.withValues(alpha: 0.35),
            blurRadius: 20,
            spreadRadius: 2,
            offset: const Offset(0, 4),
          ),
          BoxShadow(
            color: AnFitTheme.auroraPurple.withValues(alpha: 0.2),
            blurRadius: 25,
            spreadRadius: -2,
            offset: const Offset(0, -4),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onPressed,
          borderRadius: BorderRadius.circular(borderRadius),
          child: Ink(
            decoration: BoxDecoration(
              gradient: AnFitTheme.auroraGradient, // Phủ màu Aurora Gradient cao cấp
              borderRadius: BorderRadius.circular(borderRadius),
            ),
            child: Center(
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                mainAxisSize: MainAxisSize.min,
                children: [
                  if (icon != null) ...[
                    Icon(
                      icon,
                      color: const Color(0xFF1E293B), // Màu chữ tối tương phản tốt với ngọc trai
                      size: 20,
                    ),
                    const SizedBox(width: 10),
                  ],
                  Text(
                    text,
                    style: GoogleFonts.outfit(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: const Color(0xFF1E293B), // Độ tương phản xuất sắc trên nền sáng
                      letterSpacing: 0.5,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
