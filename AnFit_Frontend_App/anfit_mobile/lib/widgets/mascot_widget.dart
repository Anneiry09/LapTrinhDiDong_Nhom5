import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lottie/lottie.dart';
import '../providers/mascot_provider.dart';

/// Kiểu animation cho linh vật AnFit
enum MascotAnim {
  /// Nảy lên xuống — màn Mục tiêu / Lịch tập
  bounce,
  /// Pulse to/nhỏ như đang flex cơ — màn Vóc dáng / Thể lực
  flex,
  /// Lắc trái phải nhẹ — màn Bệnh lý / Chấn thương / Dị ứng
  wiggle,
  /// Xoay động viên — màn Rào cản
  celebrate,
  /// Gật đầu chậm — màn Hình dáng / Nghề nghiệp
  nod,
  /// Scale nhẹ như đang thở — màn Biometrics
  breathe,
}

/// Widget linh vật AnFit với animation tương tác
class MascotWidget extends ConsumerStatefulWidget {
  final MascotAnim animation;
  final double size;

  const MascotWidget({
    super.key,
    this.animation = MascotAnim.bounce,
    this.size = 110,
  });

  @override
  ConsumerState<MascotWidget> createState() => _MascotWidgetState();
}

class _MascotWidgetState extends ConsumerState<MascotWidget>
    with SingleTickerProviderStateMixin {
  late AnimationController _ctrl;
  late Animation<double> _anim;

  @override
  void initState() {
    super.initState();
    _setup();
  }

  @override
  void didUpdateWidget(MascotWidget old) {
    super.didUpdateWidget(old);
    if (old.animation != widget.animation) {
      _ctrl.dispose();
      _setup();
    }
  }

  void _setup() {
    switch (widget.animation) {
      case MascotAnim.bounce:
        _ctrl = AnimationController(vsync: this, duration: const Duration(milliseconds: 900))..repeat(reverse: true);
        _anim = Tween<double>(begin: 0, end: -14).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeInOut));
      case MascotAnim.flex:
        _ctrl = AnimationController(vsync: this, duration: const Duration(milliseconds: 650))..repeat(reverse: true);
        _anim = Tween<double>(begin: 1.0, end: 1.13).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeInOut));
      case MascotAnim.wiggle:
        _ctrl = AnimationController(vsync: this, duration: const Duration(milliseconds: 420))..repeat(reverse: true);
        _anim = Tween<double>(begin: -0.09, end: 0.09).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeInOut));
      case MascotAnim.celebrate:
        _ctrl = AnimationController(vsync: this, duration: const Duration(milliseconds: 480))..repeat(reverse: true);
        _anim = Tween<double>(begin: -0.14, end: 0.14).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeInOut));
      case MascotAnim.nod:
        _ctrl = AnimationController(vsync: this, duration: const Duration(milliseconds: 1200))..repeat(reverse: true);
        _anim = Tween<double>(begin: 0, end: -9).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeInOut));
      case MascotAnim.breathe:
        _ctrl = AnimationController(vsync: this, duration: const Duration(milliseconds: 1500))..repeat(reverse: true);
        _anim = Tween<double>(begin: 1.0, end: 1.06).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeInOut));
    }
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _anim,
      builder: (_, child) => _wrap(child!),
      child: _image(),
    );
  }

  Widget _wrap(Widget child) {
    switch (widget.animation) {
      case MascotAnim.bounce:
      case MascotAnim.nod:
        return Transform.translate(offset: Offset(0, _anim.value), child: child);
      case MascotAnim.flex:
      case MascotAnim.breathe:
        return Transform.scale(scale: _anim.value, child: child);
      case MascotAnim.wiggle:
      case MascotAnim.celebrate:
        return Transform.rotate(angle: _anim.value, child: child);
    }
  }

  /// Hiển thị Lottie linh vật động được chọn
  Widget _image() {
    final mascotState = ref.watch(mascotProvider);
    return SizedBox(
      width: widget.size,
      height: widget.size,
      child: Lottie.asset(
        'assets/images/Coacher/${mascotState.assetName}',
        width: widget.size,
        height: widget.size,
        fit: BoxFit.contain,
        repeat: true,
        animate: true,
        errorBuilder: (_, __, ___) => Icon(
          Icons.pets,
          size: widget.size * 0.6,
          color: Theme.of(context).colorScheme.primary,
        ),
      ),
    );
  }
}

/// Speech bubble gắn liền với linh vật
class MascotSpeechBubble extends StatelessWidget {
  final String text;
  const MascotSpeechBubble({super.key, required this.text});

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      decoration: BoxDecoration(
        color: isDark ? const Color(0xFF162135) : Colors.white,
        borderRadius: const BorderRadius.only(
          topLeft: Radius.circular(20),
          topRight: Radius.circular(20),
          bottomRight: Radius.circular(20),
          bottomLeft: Radius.circular(4),
        ),
        boxShadow: [
          BoxShadow(
            color: cs.primary.withValues(alpha: isDark ? 0.08 : 0.1),
            blurRadius: 14,
            offset: const Offset(0, 4),
          ),
        ],
        border: Border.all(color: cs.primary.withValues(alpha: 0.12)),
      ),
      child: Text(
        text,
        style: TextStyle(
          fontSize: 14,
          height: 1.55,
          color: cs.onSurface,
          fontWeight: FontWeight.w500,
        ),
      ),
    );
  }
}
