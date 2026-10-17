import 'dart:async';
import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:lottie/lottie.dart';
import '../core/theme/app_colors.dart';
import '../providers/mascot_provider.dart';

/// ═══════════════════════════════════════════════════════════
/// AnFitToast — 2026 Cute & Spatial Glassmorphic Notification
/// Hỗ trợ Linh vật Lottie động thay đổi theo lựa chọn của người dùng.
/// Ngăn chặn spam: Toasts mới sẽ thay thế Toasts cũ lập tức.
/// ═══════════════════════════════════════════════════════════
class AnFitToast {
  static OverlayEntry? _currentEntry;
  static Timer? _currentTimer;

  static void show(
    BuildContext context,
    String message, {
    bool isError = false,
    String? title,
    Duration duration = const Duration(milliseconds: 2800),
  }) {
    // 1. Loại bỏ Toast cũ lập tức nếu đang hiển thị (Ngăn spam hiệu quả)
    dismiss();

    // 2. Tạo OverlayEntry mới
    final overlayState = Overlay.of(context);
    final entry = OverlayEntry(
      builder: (context) {
        return _ToastWidget(
          message: message,
          isError: isError,
          title: title,
          duration: duration,
          onDismiss: () {
            dismiss();
          },
        );
      },
    );

    _currentEntry = entry;
    overlayState.insert(entry);
  }

  static void dismiss() {
    _currentTimer?.cancel();
    _currentTimer = null;
    if (_currentEntry != null) {
      _currentEntry!.remove();
      _currentEntry = null;
    }
  }
}

class _ToastWidget extends ConsumerStatefulWidget {
  final String message;
  final bool isError;
  final String? title;
  final Duration duration;
  final VoidCallback onDismiss;

  const _ToastWidget({
    required this.message,
    required this.isError,
    required this.title,
    required this.duration,
    required this.onDismiss,
  });

  @override
  ConsumerState<_ToastWidget> createState() => _ToastWidgetState();
}

class _ToastWidgetState extends ConsumerState<_ToastWidget>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  late final Animation<Offset> _offsetAnimation;
  late final Animation<double> _fadeAnimation;
  Timer? _dismissTimer;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 350),
    );

    _offsetAnimation = Tween<Offset>(
      begin: const Offset(0.0, -1.2),
      end: Offset.zero,
    ).animate(CurvedAnimation(
      parent: _controller,
      curve: Curves.easeOutBack,
    ));

    _fadeAnimation = CurvedAnimation(
      parent: _controller,
      curve: Curves.easeIn,
    );

    // Bắt đầu hoạt ảnh xuất hiện
    _controller.forward();

    // Lên lịch đóng Toast
    _dismissTimer = Timer(widget.duration, () {
      if (mounted) {
        _controller.reverse().then((_) {
          widget.onDismiss();
        });
      }
    });
  }

  @override
  void dispose() {
    _dismissTimer?.cancel();
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final mascotState = ref.watch(mascotProvider);

    // Xác định tiêu đề mặc định
    final defaultTitle = widget.title ??
        (widget.isError ? 'Lỗi rồi! ⚠️' : '${mascotState.name} nhắc bạn ✨');

    // Xác định màu sắc chủ đề dựa trên trạng thái
    final accentColor = widget.isError ? AppColors.error : AppColors.success;
    final glassColor = isDark
        ? const Color(0xE6161D25) // Midnight elevated glass
        : const Color(0xF2FFFFFF); // Soft white glass

    return SafeArea(
      child: Align(
        alignment: Alignment.topCenter,
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20.0, vertical: 10.0),
          child: SlideTransition(
            position: _offsetAnimation,
            child: FadeTransition(
              opacity: _fadeAnimation,
              child: Material(
                color: Colors.transparent,
                child: Container(
                  width: double.infinity,
                  constraints: const BoxConstraints(maxWidth: 420),
                  decoration: BoxDecoration(
                    color: glassColor,
                    borderRadius: BorderRadius.circular(24),
                    border: Border.all(
                      color: accentColor.withValues(alpha: 0.28),
                      width: 1.5,
                    ),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(
                          alpha: isDark ? 0.24 : 0.08,
                        ),
                        blurRadius: 20,
                        offset: const Offset(0, 8),
                      ),
                    ],
                  ),
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(24),
                    child: BackdropFilter(
                      filter: ImageFilter.blur(sigmaX: 12, sigmaY: 12),
                      child: Padding(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 16.0,
                          vertical: 14.0,
                        ),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.center,
                          children: [
                            // ── LINH VẬT ĐỘNG LOTTIE CỰC CUTE ──
                            Container(
                              width: 52,
                              height: 52,
                              decoration: BoxDecoration(
                                color: accentColor.withValues(alpha: 0.1),
                                shape: BoxShape.circle,
                              ),
                              alignment: Alignment.center,
                              child: ClipOval(
                                child: Lottie.asset(
                                  'assets/images/Coacher/${mascotState.assetName}',
                                  width: 48,
                                  height: 48,
                                  fit: BoxFit.contain,
                                  repeat: true,
                                  animate: true,
                                ),
                              ),
                            ),
                            const SizedBox(width: 14),

                            // ── NỘI DUNG THÔNG BÁO ──
                            Expanded(
                              child: Column(
                                mainAxisSize: MainAxisSize.min,
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    defaultTitle,
                                    style: GoogleFonts.outfit(
                                      fontSize: 14,
                                      fontWeight: FontWeight.bold,
                                      color: accentColor,
                                      letterSpacing: 0.2,
                                    ),
                                  ),
                                  const SizedBox(height: 3),
                                  Text(
                                    widget.message,
                                    style: GoogleFonts.inter(
                                      fontSize: 13,
                                      fontWeight: FontWeight.w500,
                                      color: isDark
                                          ? AppColors.darkText
                                          : AppColors.lightText,
                                      height: 1.35,
                                    ),
                                    maxLines: 3,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
