import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:google_fonts/google_fonts.dart';
import 'app_colors.dart';

/// ============================================================================
/// AppTheme — 2026 Premium Wellness & Spatial Design
/// ============================================================================
class AppTheme {
  // ─── Shared border radii ───────────────────────────────────────────────────
  static const double radiusCard = 24.0;
  static const double radiusBtn = 18.0;
  static const double radiusInput = 16.0;
  static const double radiusChip = 12.0;

  // ─── Text themes (Outfit headings + Inter body) ────────────────────────────
  static TextTheme _buildTextTheme(Color bodyColor) {
    return TextTheme(
      displayLarge:
          GoogleFonts.outfit(fontSize: 48, fontWeight: FontWeight.w800, color: bodyColor, letterSpacing: -1.5),
      displayMedium:
          GoogleFonts.outfit(fontSize: 40, fontWeight: FontWeight.w700, color: bodyColor, letterSpacing: -1.0),
      displaySmall:
          GoogleFonts.outfit(fontSize: 32, fontWeight: FontWeight.w700, color: bodyColor, letterSpacing: -0.5),
      headlineLarge:
          GoogleFonts.outfit(fontSize: 28, fontWeight: FontWeight.bold, color: bodyColor, letterSpacing: -0.5),
      headlineMedium:
          GoogleFonts.outfit(fontSize: 24, fontWeight: FontWeight.w600, color: bodyColor),
      headlineSmall:
          GoogleFonts.outfit(fontSize: 20, fontWeight: FontWeight.w600, color: bodyColor),
      titleLarge:
          GoogleFonts.outfit(fontSize: 18, fontWeight: FontWeight.w600, color: bodyColor),
      titleMedium:
          GoogleFonts.outfit(fontSize: 16, fontWeight: FontWeight.w600, color: bodyColor),
      titleSmall:
          GoogleFonts.outfit(fontSize: 14, fontWeight: FontWeight.w600, color: bodyColor),
      bodyLarge:
          GoogleFonts.inter(fontSize: 16, fontWeight: FontWeight.normal, color: bodyColor),
      bodyMedium:
          GoogleFonts.inter(fontSize: 14, fontWeight: FontWeight.normal, color: bodyColor),
      bodySmall:
          GoogleFonts.inter(fontSize: 12, fontWeight: FontWeight.normal, color: bodyColor),
      labelLarge:
          GoogleFonts.inter(fontSize: 14, fontWeight: FontWeight.w600, color: bodyColor, letterSpacing: 0.3),
      labelMedium:
          GoogleFonts.inter(fontSize: 12, fontWeight: FontWeight.w500, color: bodyColor),
      labelSmall:
          GoogleFonts.inter(fontSize: 10, fontWeight: FontWeight.w500, color: bodyColor, letterSpacing: 0.5),
    );
  }

  // ─── LIGHT THEME ──────────────────────────────────────────────────────────
  static ThemeData get lightTheme {
    final base = ThemeData(brightness: Brightness.light);
    return base.copyWith(
      useMaterial3: true,
      primaryColor: AppColors.lightPrimary,
      scaffoldBackgroundColor: AppColors.lightBg,
      colorScheme: const ColorScheme.light(
        primary: AppColors.lightPrimary,
        primaryContainer: Color(0xFFD1FAE5),
        onPrimaryContainer: Color(0xFF065F46),
        secondary: AppColors.lightCoral,
        secondaryContainer: Color(0xFFFFE4E4),
        surface: AppColors.lightSurface,
        surfaceContainerHighest: AppColors.lightCard,
        onSurface: AppColors.lightText,
        onSurfaceVariant: AppColors.lightTextSub,
        outline: AppColors.lightDivider,
        error: AppColors.error,
        tertiary: AppColors.auroraPurple,
        shadow: Color(0xFF000000),
      ),
      textTheme: _buildTextTheme(AppColors.lightText),
      appBarTheme: AppBarTheme(
        backgroundColor: AppColors.lightSurface,
        foregroundColor: AppColors.lightText,
        elevation: 0,
        surfaceTintColor: Colors.transparent,
        systemOverlayStyle: SystemUiOverlayStyle.dark,
        titleTextStyle: GoogleFonts.outfit(
          fontSize: 18,
          fontWeight: FontWeight.w600,
          color: AppColors.lightText,
        ),
      ),
      navigationBarTheme: NavigationBarThemeData(
        backgroundColor: AppColors.lightSurface,
        elevation: 0,
        height: 68,
        indicatorColor: AppColors.lightPrimary.withValues(alpha: 0.12),
        iconTheme: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return const IconThemeData(color: AppColors.lightPrimary, size: 24);
          }
          return const IconThemeData(color: AppColors.lightTextSub, size: 22);
        }),
        labelTextStyle: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return GoogleFonts.inter(
              color: AppColors.lightPrimary,
              fontWeight: FontWeight.w600,
              fontSize: 11,
            );
          }
          return GoogleFonts.inter(
            color: AppColors.lightTextSub,
            fontWeight: FontWeight.w500,
            fontSize: 11,
          );
        }),
      ),
      cardTheme: CardThemeData(
        color: AppColors.lightSurface,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(radiusCard),
          side: const BorderSide(color: AppColors.lightDivider, width: 0.8),
        ),
        margin: EdgeInsets.zero,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: AppColors.lightPrimary,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(radiusBtn)),
          padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 24),
          textStyle: GoogleFonts.outfit(fontWeight: FontWeight.bold, fontSize: 15, letterSpacing: 0.3),
          elevation: 0,
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: AppColors.lightPrimary,
          side: const BorderSide(color: AppColors.lightPrimary, width: 1.5),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(radiusBtn)),
          padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 24),
          textStyle: GoogleFonts.outfit(fontWeight: FontWeight.w600, fontSize: 15),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          backgroundColor: AppColors.lightPrimary,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(radiusBtn)),
          padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 24),
          textStyle: GoogleFonts.outfit(fontWeight: FontWeight.bold, fontSize: 15),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: AppColors.lightCard,
        contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusInput),
          borderSide: BorderSide.none,
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusInput),
          borderSide: const BorderSide(color: AppColors.lightDivider, width: 0.8),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusInput),
          borderSide: const BorderSide(color: AppColors.lightPrimary, width: 2),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusInput),
          borderSide: const BorderSide(color: AppColors.error, width: 1.5),
        ),
        prefixIconColor: AppColors.lightTextSub,
        hintStyle: GoogleFonts.inter(color: AppColors.lightTextSub, fontSize: 14),
      ),
      chipTheme: ChipThemeData(
        backgroundColor: AppColors.lightCard,
        selectedColor: AppColors.lightPrimary.withValues(alpha: 0.15),
        side: const BorderSide(color: Colors.transparent),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(radiusChip)),
        labelStyle: GoogleFonts.inter(fontSize: 13, fontWeight: FontWeight.w500),
      ),
      dividerTheme: const DividerThemeData(color: AppColors.lightDivider, thickness: 0.8),
      sliderTheme: const SliderThemeData(
        activeTrackColor: AppColors.lightPrimary,
        thumbColor: AppColors.lightPrimary,
      ),
      snackBarTheme: SnackBarThemeData(
        backgroundColor: const Color(0xFF1C2530),
        contentTextStyle: GoogleFonts.inter(color: Colors.white, fontSize: 14),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
        behavior: SnackBarBehavior.floating,
        insetPadding: const EdgeInsets.all(16),
      ),
      dialogTheme: DialogThemeData(
        backgroundColor: AppColors.lightSurface,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(radiusCard)),
        elevation: 4,
      ),
      bottomSheetTheme: const BottomSheetThemeData(
        backgroundColor: AppColors.lightSurface,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
        ),
        elevation: 0,
      ),
    );
  }

  // ─── DARK THEME ───────────────────────────────────────────────────────────
  static ThemeData get darkTheme {
    final base = ThemeData(brightness: Brightness.dark);
    return base.copyWith(
      useMaterial3: true,
      primaryColor: AppColors.darkPrimary,
      scaffoldBackgroundColor: AppColors.darkBg,
      colorScheme: const ColorScheme.dark(
        primary: AppColors.darkPrimary,
        primaryContainer: Color(0xFF064E3B),
        onPrimaryContainer: AppColors.darkPrimaryGlow,
        secondary: AppColors.darkCoral,
        secondaryContainer: Color(0xFF3D1515),
        onSecondaryContainer: AppColors.darkCoralGlow,
        surface: AppColors.darkSurface,
        surfaceContainerHighest: AppColors.darkCard,
        onSurface: AppColors.darkText,
        onSurfaceVariant: AppColors.darkTextSub,
        outline: AppColors.darkDivider,
        error: AppColors.error,
        tertiary: AppColors.auroraPurple,
        shadow: Color(0xFF000000),
      ),
      textTheme: _buildTextTheme(AppColors.darkText),
      appBarTheme: AppBarTheme(
        backgroundColor: Colors.transparent,
        foregroundColor: AppColors.darkText,
        elevation: 0,
        surfaceTintColor: Colors.transparent,
        systemOverlayStyle: SystemUiOverlayStyle.light,
        titleTextStyle: GoogleFonts.outfit(
          fontSize: 18,
          fontWeight: FontWeight.w600,
          color: AppColors.darkText,
        ),
      ),
      navigationBarTheme: NavigationBarThemeData(
        backgroundColor: AppColors.darkSurface,
        elevation: 0,
        height: 68,
        indicatorColor: AppColors.darkPrimary.withValues(alpha: 0.15),
        iconTheme: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return const IconThemeData(color: AppColors.darkPrimary, size: 24);
          }
          return const IconThemeData(color: AppColors.darkTextSub, size: 22);
        }),
        labelTextStyle: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return GoogleFonts.inter(
              color: AppColors.darkPrimary,
              fontWeight: FontWeight.w600,
              fontSize: 11,
            );
          }
          return GoogleFonts.inter(
            color: AppColors.darkTextSub,
            fontWeight: FontWeight.w500,
            fontSize: 11,
          );
        }),
      ),
      cardTheme: CardThemeData(
        color: AppColors.darkCard,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(radiusCard),
          side: BorderSide(
            color: Colors.white.withValues(alpha: 0.06),
            width: 0.8,
          ),
        ),
        margin: EdgeInsets.zero,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: AppColors.darkPrimary,
          foregroundColor: AppColors.darkBg,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(radiusBtn)),
          padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 24),
          textStyle: GoogleFonts.outfit(fontWeight: FontWeight.bold, fontSize: 15, letterSpacing: 0.3),
          elevation: 0,
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: AppColors.darkPrimary,
          side: const BorderSide(color: AppColors.darkPrimary, width: 1.5),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(radiusBtn)),
          padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 24),
          textStyle: GoogleFonts.outfit(fontWeight: FontWeight.w600, fontSize: 15),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          backgroundColor: AppColors.darkPrimary,
          foregroundColor: AppColors.darkBg,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(radiusBtn)),
          padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 24),
          textStyle: GoogleFonts.outfit(fontWeight: FontWeight.bold, fontSize: 15),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: AppColors.darkCard,
        contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusInput),
          borderSide: BorderSide.none,
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusInput),
          borderSide: BorderSide(color: Colors.white.withValues(alpha: 0.08), width: 0.8),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusInput),
          borderSide: const BorderSide(color: AppColors.darkPrimary, width: 2),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusInput),
          borderSide: const BorderSide(color: AppColors.error, width: 1.5),
        ),
        prefixIconColor: AppColors.darkTextSub,
        hintStyle: GoogleFonts.inter(color: AppColors.darkTextSub, fontSize: 14),
      ),
      chipTheme: ChipThemeData(
        backgroundColor: AppColors.darkCard,
        selectedColor: AppColors.darkPrimary.withValues(alpha: 0.18),
        side: BorderSide(color: Colors.white.withValues(alpha: 0.06)),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(radiusChip)),
        labelStyle: GoogleFonts.inter(
          fontSize: 13,
          fontWeight: FontWeight.w500,
          color: AppColors.darkText,
        ),
      ),
      dividerTheme: const DividerThemeData(color: AppColors.darkDivider, thickness: 0.8),
      sliderTheme: const SliderThemeData(
        activeTrackColor: AppColors.darkPrimary,
        thumbColor: AppColors.darkPrimary,
      ),
      snackBarTheme: SnackBarThemeData(
        backgroundColor: AppColors.darkCard,
        contentTextStyle: GoogleFonts.inter(color: AppColors.darkText, fontSize: 14),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
        behavior: SnackBarBehavior.floating,
        insetPadding: const EdgeInsets.all(16),
      ),
      dialogTheme: DialogThemeData(
        backgroundColor: AppColors.darkCard,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(radiusCard)),
        elevation: 0,
      ),
      bottomSheetTheme: BottomSheetThemeData(
        backgroundColor: AppColors.darkSurface,
        shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
        ),
        elevation: 0,
        surfaceTintColor: Colors.transparent,
        dragHandleColor: AppColors.darkDivider,
        dragHandleSize: const Size(40, 4),
      ),
    );
  }
}

/// ============================================================================
/// GlassCard — Widget kính mờ tái sử dụng toàn bộ app
/// ============================================================================
class GlassCard extends StatelessWidget {
  final Widget child;
  final double borderRadius;
  final double blurSigma;
  final EdgeInsetsGeometry? padding;
  final EdgeInsetsGeometry? margin;
  final double? width;
  final double? height;
  final Color? tintColor;
  final double tintOpacity;
  final Color? borderColor;
  final VoidCallback? onTap;

  const GlassCard({
    super.key,
    required this.child,
    this.borderRadius = 24.0,
    this.blurSigma = 12.0,
    this.padding,
    this.margin,
    this.width,
    this.height,
    this.tintColor,
    this.tintOpacity = 0.06,
    this.borderColor,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final effectiveTint = tintColor ?? Colors.white;
    final effectiveBorder = borderColor ??
        Colors.white.withValues(alpha: isDark ? 0.08 : 0.6);

    Widget card = Container(
      width: width,
      height: height,
      margin: margin,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(borderRadius),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: isDark ? 0.18 : 0.06),
            blurRadius: 24,
            spreadRadius: -4,
            offset: const Offset(0, 8),
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
              color: effectiveTint.withValues(alpha: tintOpacity),
              border: Border.all(color: effectiveBorder, width: 0.8),
            ),
            child: child,
          ),
        ),
      ),
    );

    if (onTap != null) {
      return GestureDetector(onTap: onTap, child: card);
    }
    return card;
  }
}

/// ============================================================================
/// AuroraButton — Nút bấm Aurora gradient cho tính năng AI
/// ============================================================================
class AuroraButton extends StatelessWidget {
  final String text;
  final VoidCallback onPressed;
  final IconData? icon;
  final double height;
  final double borderRadius;
  final bool isLoading;

  const AuroraButton({
    super.key,
    required this.text,
    required this.onPressed,
    this.icon,
    this.height = 56.0,
    this.borderRadius = 18.0,
    this.isLoading = false,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      height: height,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(borderRadius),
        boxShadow: [
          BoxShadow(
            color: AppColors.auroraCyan.withValues(alpha: 0.3),
            blurRadius: 20,
            spreadRadius: 2,
            offset: const Offset(0, 4),
          ),
          BoxShadow(
            color: AppColors.auroraPurple.withValues(alpha: 0.2),
            blurRadius: 25,
            spreadRadius: -2,
            offset: const Offset(0, -2),
          ),
        ],
        gradient: const LinearGradient(
          colors: [AppColors.auroraPurple, AppColors.auroraCyan],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: isLoading ? null : onPressed,
          borderRadius: BorderRadius.circular(borderRadius),
          child: Center(
            child: isLoading
                ? const SizedBox(
                    width: 22,
                    height: 22,
                    child: CircularProgressIndicator(
                      strokeWidth: 2.5,
                      color: Colors.white,
                    ),
                  )
                : Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      if (icon != null) ...[
                        Icon(icon, color: Colors.white, size: 20),
                        const SizedBox(width: 10),
                      ],
                      Text(
                        text,
                        style: GoogleFonts.outfit(
                          fontSize: 15,
                          fontWeight: FontWeight.bold,
                          color: Colors.white,
                          letterSpacing: 0.3,
                        ),
                      ),
                    ],
                  ),
          ),
        ),
      ),
    );
  }
}

/// ============================================================================
/// StatPill — Widget hiển thị metric nhỏ (steps, heart, sleep)
/// ============================================================================
class StatPill extends StatelessWidget {
  final IconData icon;
  final String value;
  final String label;
  final Color color;
  final VoidCallback? onTap;

  const StatPill({
    super.key,
    required this.icon,
    required this.value,
    required this.label,
    required this.color,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    Widget pill = Container(
      padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 8),
      decoration: BoxDecoration(
        color: color.withValues(alpha: isDark ? 0.08 : 0.06),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: color.withValues(alpha: 0.18), width: 0.8),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 36,
            height: 36,
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.12),
              shape: BoxShape.circle,
            ),
            child: Icon(icon, size: 18, color: color),
          ),
          const SizedBox(height: 8),
          Text(
            value,
            style: GoogleFonts.outfit(
              fontWeight: FontWeight.bold,
              fontSize: 16,
              color: isDark ? AppColors.darkText : AppColors.lightText,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            label,
            style: GoogleFonts.inter(
              fontSize: 10,
              color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
              fontWeight: FontWeight.w500,
            ),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );

    if (onTap != null) {
      pill = GestureDetector(onTap: onTap, child: pill);
    }

    return Expanded(child: pill);
  }
}
