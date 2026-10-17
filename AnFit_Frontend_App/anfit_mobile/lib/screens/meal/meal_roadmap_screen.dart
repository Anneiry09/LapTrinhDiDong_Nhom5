import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import 'meal_plan_screen.dart';
import '../../widgets/anfit_toast.dart';

/// ═══════════════════════════════════════════════════════════
/// MealRoadmapScreen — Premium Lộ Trình Dinh Dưỡng 30 Ngày
/// ═══════════════════════════════════════════════════════════
class MealRoadmapScreen extends ConsumerStatefulWidget {
  const MealRoadmapScreen({super.key});
  @override
  ConsumerState<MealRoadmapScreen> createState() => _MealRoadmapScreenState();
}

class _MealRoadmapScreenState extends ConsumerState<MealRoadmapScreen>
    with SingleTickerProviderStateMixin {
  bool _loading = true;
  bool _generating = false;
  String? _error;
  List<Map<String, dynamic>> _plans = [];
  int? _selectedDayIndex;
  late AnimationController _animCtrl;

  @override
  void initState() {
    super.initState();
    _animCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 400),
    );
    _loadRoadmap();
  }

  @override
  void dispose() {
    _animCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadRoadmap() async {
    setState(() { _loading = true; _error = null; });
    try {
      final data = await ref.read(nutritionServiceProvider).getRoadmap();
      setState(() {
        _plans = data.cast<Map<String, dynamic>>();
        _loading = false;
      });
      _animCtrl.forward(from: 0);
    } on DioException catch (e) {
      setState(() { _loading = false; _error = ApiClient.errorMessage(e); });
    } catch (e) {
      setState(() { _loading = false; _error = 'Không thể tải lộ trình.'; });
    }
  }

  Future<void> _generateRoadmap({bool force = false}) async {
    setState(() => _generating = true);
    try {
      await ref.read(nutritionServiceProvider).generateRoadmap(force: force);
      if (mounted) {
        _showToast(context, '✅ Đã tạo lộ trình 30 ngày thành công!', isSuccess: true);
      }
      await _loadRoadmap();
    } on DioException catch (e) {
      final msg = ApiClient.errorMessage(e);
      if (msg == 'ROADMAP_INCOMPLETE') {
        if (mounted) {
          _showIncompleteWarningDialog();
        }
      } else {
        if (mounted) _showToast(context, msg, isSuccess: false);
      }
    } catch (e) {
      if (mounted) _showToast(context, 'Lỗi tạo lộ trình, thử lại.', isSuccess: false);
    } finally {
      if (mounted) setState(() => _generating = false);
    }
  }

  void _showIncompleteWarningDialog() {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final textPrimary = isDark ? AppColors.darkText : AppColors.lightText;
    final textSub = isDark ? AppColors.darkTextSub : AppColors.lightTextSub;
    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: isDark ? AppColors.darkSurface : AppColors.lightSurface,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
        title: Row(
          children: [
            Icon(Icons.warning_amber_rounded, color: AppColors.warning, size: 28),
            const SizedBox(width: 10),
            Expanded(
              child: Text(
                'Chưa Hoàn Thành Lộ Trình!',
                style: GoogleFonts.outfit(
                  fontWeight: FontWeight.bold,
                  fontSize: 18,
                  color: textPrimary,
                ),
              ),
            ),
          ],
        ),
        content: Text(
          'Lộ trình 30 ngày hiện tại của bạn chưa hoàn thành 100%. Tạo lộ trình mới sẽ đóng lộ trình cũ (lưu các ngày đã hoàn thành vào lịch sử) và khởi tạo một lộ trình mới bắt đầu từ hôm nay.\n\nBạn có chắc chắn muốn tiếp tục?',
          style: GoogleFonts.inter(
            fontSize: 14,
            color: textSub,
            height: 1.5,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: Text(
              'Hủy',
              style: GoogleFonts.inter(
                color: textSub,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(ctx);
              _generateRoadmap(force: true);
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: primary,
              foregroundColor: isDark ? AppColors.darkBg : Colors.white,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
            child: Text(
              'Tiếp tục tạo mới',
              style: GoogleFonts.inter(
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ],
      ),
    );
  }



  void _openDayDetail(int dayIndex) {
    final plan = _plans[dayIndex];
    final date = plan['planDate'] as String?;
    Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => MealPlanScreen(date: date)),
    ).then((_) {
      _loadRoadmap();
    });
  }

  void _showToast(BuildContext context, String message, {required bool isSuccess}) {
    AnFitToast.show(context, message, isError: !isSuccess);
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Scaffold(
      backgroundColor: isDark ? AppColors.darkBg : AppColors.lightBg,
      body: CustomScrollView(
        slivers: [
          // ── AppBar ──────────────────────────────────────────
          SliverAppBar(
            pinned: true,
            backgroundColor: isDark ? AppColors.darkSurface : AppColors.lightSurface,
            surfaceTintColor: Colors.transparent,
            automaticallyImplyLeading: false,
            title: Text(
              'Dinh Dưỡng 30 Ngày',
              style: GoogleFonts.outfit(
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: isDark ? AppColors.darkText : AppColors.lightText,
              ),
            ),
          ),

          // ── Body ────────────────────────────────────────────
          if (_loading)
            SliverFillRemaining(child: _buildSkeleton(isDark))
          else if (_error != null)
            SliverFillRemaining(child: _buildError(isDark))
          else if (_plans.isEmpty)
            SliverFillRemaining(child: _buildEmpty(isDark))
          else
            SliverList(
              delegate: SliverChildListDelegate([
                // Progress header
                _buildProgressHeader(isDark),
                // Grid 30 ngày
                _buildGrid(isDark),
                // Day detail expanded
                AnimatedSize(
                  duration: const Duration(milliseconds: 280),
                  curve: Curves.easeOut,
                  child: _selectedDayIndex != null
                      ? _buildDayDetail(isDark)
                      : const SizedBox.shrink(),
                ),
                // Generate button
                _buildGenerateButton(isDark),
                const SizedBox(height: 80), // bottom nav padding
              ]),
            ),
        ],
      ),
    );
  }

  Widget _buildSkeleton(bool isDark) {
    final base = isDark ? AppColors.darkCard : AppColors.lightCard;
    return Padding(
      padding: const EdgeInsets.all(20),
      child: Column(
        children: [
          Container(height: 80, decoration: BoxDecoration(color: base, borderRadius: BorderRadius.circular(20))),
          const SizedBox(height: 16),
          GridView.builder(
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: 5,
              mainAxisSpacing: 10,
              crossAxisSpacing: 10,
              childAspectRatio: 0.85,
            ),
            itemCount: 10,
            itemBuilder: (_, __) => Container(
              decoration: BoxDecoration(color: base, borderRadius: BorderRadius.circular(14)),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildError(bool isDark) {
    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    return Center(
      child: Column(mainAxisSize: MainAxisSize.min, children: [
        Icon(Icons.wifi_off_rounded, size: 52, color: AppColors.error.withValues(alpha: 0.5)),
        const SizedBox(height: 16),
        Text(_error!, style: GoogleFonts.inter(color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub)),
        const SizedBox(height: 20),
        GestureDetector(
          onTap: _loadRoadmap,
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
            decoration: BoxDecoration(
              color: primary.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(14),
              border: Border.all(color: primary.withValues(alpha: 0.3)),
            ),
            child: Text('Thử lại', style: GoogleFonts.inter(color: primary, fontWeight: FontWeight.w600)),
          ),
        ),
      ]),
    );
  }

  Widget _buildEmpty(bool isDark) {
    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    final textPrimary = isDark ? AppColors.darkText : AppColors.lightText;
    final textSub = isDark ? AppColors.darkTextSub : AppColors.lightTextSub;

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Container(
            width: 80, height: 80,
            decoration: BoxDecoration(
              color: primary.withValues(alpha: 0.1),
              shape: BoxShape.circle,
            ),
            child: Icon(Icons.restaurant_menu_rounded, size: 40, color: primary),
          ),
          const SizedBox(height: 20),
          Text(
            'Chưa có lộ trình dinh dưỡng',
            style: GoogleFonts.outfit(fontSize: 20, fontWeight: FontWeight.bold, color: textPrimary),
          ),
          const SizedBox(height: 8),
          Text(
            'Để AI lên kế hoạch ăn uống\nkhoa học cho 30 ngày!',
            textAlign: TextAlign.center,
            style: GoogleFonts.inter(fontSize: 14, color: textSub, height: 1.5),
          ),
          const SizedBox(height: 32),
          _buildGenerateButton(isDark),
        ]),
      ),
    );
  }

  Widget _buildProgressHeader(bool isDark) {
    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    final textPrimary = isDark ? AppColors.darkText : AppColors.lightText;
    final textSub = isDark ? AppColors.darkTextSub : AppColors.lightTextSub;
    final cardBg = isDark ? AppColors.darkCard : AppColors.lightSurface;

    final completedCount = _plans.where((p) => (p['status'] as int? ?? 0) == 1).length;
    final progress = _plans.isNotEmpty ? completedCount / _plans.length : 0.0;

    return Container(
      margin: const EdgeInsets.fromLTRB(16, 12, 16, 0),
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: cardBg,
        borderRadius: BorderRadius.circular(24),
        border: Border.all(
          color: isDark ? Colors.white.withValues(alpha: 0.07) : Colors.black.withValues(alpha: 0.05),
          width: 0.8,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: isDark ? 0.15 : 0.04),
            blurRadius: 16,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 40, height: 40,
                decoration: BoxDecoration(
                  color: primary.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(Icons.emoji_events_rounded, color: primary, size: 22),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(
                    'Tiến trình lộ trình',
                    style: GoogleFonts.outfit(
                      fontWeight: FontWeight.bold,
                      fontSize: 15,
                      color: textPrimary,
                    ),
                  ),
                  Text(
                    '$completedCount / ${_plans.length} ngày hoàn tất',
                    style: GoogleFonts.inter(fontSize: 12, color: textSub),
                  ),
                ]),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: primary.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Text(
                  '${(progress * 100).toStringAsFixed(0)}%',
                  style: GoogleFonts.outfit(
                    fontWeight: FontWeight.bold,
                    color: primary,
                    fontSize: 16,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          ClipRRect(
            borderRadius: BorderRadius.circular(8),
            child: LinearProgressIndicator(
              value: progress,
              minHeight: 8,
              backgroundColor: isDark
                  ? Colors.white.withValues(alpha: 0.06)
                  : Colors.black.withValues(alpha: 0.05),
              valueColor: AlwaysStoppedAnimation<Color>(primary),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildGrid(bool isDark) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 0),
      child: GridView.builder(
        shrinkWrap: true,
        physics: const NeverScrollableScrollPhysics(),
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 5,
          mainAxisSpacing: 10,
          crossAxisSpacing: 10,
          childAspectRatio: 0.82,
        ),
        itemCount: _plans.length,
        itemBuilder: (context, index) => _buildDayTile(index, isDark),
      ),
    );
  }

  Widget _buildDayTile(int index, bool isDark) {
    final plan = _plans[index];
    final status = plan['status'] as int? ?? 0;
    final isCompleted = status == 1;
    final isSelected = _selectedDayIndex == index;
    final dateStr = plan['planDate'] as String? ?? '';
    final dayNum = index + 1;
    final dayLabel = dateStr.length >= 10 ? dateStr.substring(8, 10) : '$dayNum';

    final todayStr = DateTime.now().toIso8601String().substring(0, 10);
    final isPast = dateStr.isNotEmpty && dateStr.compareTo(todayStr) < 0;
    final isMissed = isPast && !isCompleted;

    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    final coral = isDark ? AppColors.darkCoral : AppColors.lightCoral;

    Color bgColor;
    Color borderColor;
    Color textColor;

    if (isSelected) {
      bgColor = coral;
      borderColor = coral;
      textColor = Colors.white;
    } else if (isCompleted) {
      bgColor = AppColors.success.withValues(alpha: isDark ? 0.12 : 0.1);
      borderColor = AppColors.success.withValues(alpha: 0.35);
      textColor = AppColors.success;
    } else if (isMissed) {
      bgColor = AppColors.error.withValues(alpha: isDark ? 0.12 : 0.1);
      borderColor = AppColors.error.withValues(alpha: 0.35);
      textColor = AppColors.error;
    } else {
      bgColor = isDark ? Colors.white.withValues(alpha: 0.04) : Colors.black.withValues(alpha: 0.025);
      borderColor = isDark ? Colors.white.withValues(alpha: 0.08) : Colors.black.withValues(alpha: 0.06);
      textColor = isDark ? AppColors.darkTextSub : AppColors.lightTextSub;
    }

    return GestureDetector(
      onTap: () => setState(() {
        _selectedDayIndex = _selectedDayIndex == index ? null : index;
      }),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        curve: Curves.easeOut,
        decoration: BoxDecoration(
          color: bgColor,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: borderColor, width: isSelected ? 0 : 0.8),
          boxShadow: isSelected
              ? [BoxShadow(color: coral.withValues(alpha: 0.3), blurRadius: 8, offset: const Offset(0, 2))]
              : null,
        ),
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
          Text(
            'N$dayNum',
            style: GoogleFonts.inter(
              fontSize: 9,
              fontWeight: FontWeight.w600,
              color: isSelected ? Colors.white.withValues(alpha: 0.8) : textColor.withValues(alpha: 0.6),
            ),
          ),
          const SizedBox(height: 1),
          Text(
            dayLabel,
            style: GoogleFonts.outfit(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: textColor,
            ),
          ),
          const SizedBox(height: 4),
          if (isCompleted)
            Icon(
              Icons.check_circle_rounded,
              color: isSelected ? Colors.white : AppColors.success,
              size: 16,
            )
          else if (isMissed)
            Icon(
              Icons.close_rounded,
              color: isSelected ? Colors.white : AppColors.error,
              size: 16,
            )
          else if (isSelected)
            Icon(Icons.restaurant_rounded, color: Colors.white, size: 14)
          else
            Container(
              width: 6, height: 6,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: primary.withValues(alpha: 0.2),
              ),
            ),
        ]),
      ),
    );
  }

  Widget _buildDayDetail(bool isDark) {
    final plan = _plans[_selectedDayIndex!];
    final dayNum = _selectedDayIndex! + 1;
    final dateStr = plan['planDate'] as String? ?? '';
    final totalCal = (plan['totalCalories'] ?? 0).toDouble();
    final totalP = (plan['totalProtein'] ?? 0).toDouble();
    final totalC = (plan['totalCarbs'] ?? 0).toDouble();
    final totalF = (plan['totalFat'] ?? 0).toDouble();
    final totalPrice = plan['totalPrice'] ?? 0;
    final status = plan['status'] as int? ?? 0;
    final isCompleted = status == 1;

    final todayStr = DateTime.now().toIso8601String().substring(0, 10);
    final isPast = dateStr.isNotEmpty && dateStr.compareTo(todayStr) < 0;
    final isMissed = isPast && !isCompleted;

    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    final coral = isDark ? AppColors.darkCoral : AppColors.lightCoral;
    final textPrimary = isDark ? AppColors.darkText : AppColors.lightText;
    final textSub = isDark ? AppColors.darkTextSub : AppColors.lightTextSub;
    final cardBg = isDark ? AppColors.darkCard : AppColors.lightSurface;

    return Container(
      margin: const EdgeInsets.fromLTRB(16, 12, 16, 0),
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: cardBg,
        borderRadius: BorderRadius.circular(22),
        border: Border.all(
          color: isDark ? Colors.white.withValues(alpha: 0.08) : Colors.black.withValues(alpha: 0.05),
          width: 0.8,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: isDark ? 0.2 : 0.06),
            blurRadius: 16,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        // Header row
        Row(children: [
          Container(
            width: 36, height: 36,
            decoration: BoxDecoration(
              color: coral.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(Icons.calendar_today_rounded, size: 18, color: coral),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(
                'Ngày $dayNum — $dateStr',
                style: GoogleFonts.outfit(fontWeight: FontWeight.bold, fontSize: 14, color: textPrimary),
              ),
            ]),
          ),
          if (isCompleted)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(
                color: AppColors.success.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Row(mainAxisSize: MainAxisSize.min, children: [
                const Icon(Icons.check_circle_rounded, color: AppColors.success, size: 13),
                const SizedBox(width: 4),
                Text(
                  'Hoàn tất',
                  style: GoogleFonts.inter(fontSize: 11, color: AppColors.success, fontWeight: FontWeight.w600),
                ),
              ]),
            )
          else if (isMissed)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(
                color: AppColors.error.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Row(mainAxisSize: MainAxisSize.min, children: [
                const Icon(Icons.close_rounded, color: AppColors.error, size: 13),
                const SizedBox(width: 4),
                Text(
                  'Bỏ lỡ',
                  style: GoogleFonts.inter(fontSize: 11, color: AppColors.error, fontWeight: FontWeight.w600),
                ),
              ]),
            ),
        ]),
        const SizedBox(height: 14),

        // Macro chips
        Row(children: [
          _MacroChip(label: '${totalCal.toStringAsFixed(0)} kcal', color: primary),
          const SizedBox(width: 6),
          _MacroChip(label: 'P ${totalP.toStringAsFixed(0)}g', color: AppColors.success),
          const SizedBox(width: 6),
          _MacroChip(label: 'C ${totalC.toStringAsFixed(0)}g', color: AppColors.warning),
          const SizedBox(width: 6),
          _MacroChip(label: 'F ${totalF.toStringAsFixed(0)}g', color: AppColors.error),
        ]),
        const SizedBox(height: 8),
        Text(
          '${(totalPrice is num ? totalPrice / 1000 : 0).toStringAsFixed(0)}K VNĐ/ngày',
          style: GoogleFonts.inter(fontSize: 11, color: textSub),
        ),
        const SizedBox(height: 14),

        // Actions
        Row(children: [
          Expanded(
            child: OutlinedButton.icon(
              onPressed: () => _openDayDetail(_selectedDayIndex!),
              icon: const Icon(Icons.restaurant_menu_rounded, size: 16),
              label: const Text('Xem chi tiết'),
              style: OutlinedButton.styleFrom(
                foregroundColor: coral,
                side: BorderSide(color: coral.withValues(alpha: 0.4)),
                padding: const EdgeInsets.symmetric(vertical: 10),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                textStyle: GoogleFonts.inter(fontWeight: FontWeight.w600, fontSize: 13),
              ),
            ),
          ),
        ]),
      ]),
    );
  }

  Widget _buildGenerateButton(bool isDark) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 0),
      child: GestureDetector(
        onTap: _generating ? null : _generateRoadmap,
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
          height: 54,
          decoration: BoxDecoration(
            gradient: _generating
                ? null
                : const LinearGradient(
                    colors: [AppColors.darkCoral, Color(0xFFFFB347)],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
            color: _generating ? (isDark ? AppColors.darkCard : AppColors.lightCard) : null,
            borderRadius: BorderRadius.circular(18),
            boxShadow: _generating
                ? null
                : [
                    BoxShadow(
                      color: AppColors.darkCoral.withValues(alpha: 0.3),
                      blurRadius: 16,
                      offset: const Offset(0, 4),
                    ),
                  ],
          ),
          child: Center(
            child: _generating
                ? Row(mainAxisSize: MainAxisSize.min, children: [
                    SizedBox(
                      width: 18, height: 18,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: isDark ? AppColors.darkPrimary : AppColors.lightPrimary,
                      ),
                    ),
                    const SizedBox(width: 10),
                    Text(
                      'AI đang tạo lộ trình...',
                      style: GoogleFonts.outfit(
                        fontWeight: FontWeight.w600,
                        color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                        fontSize: 14,
                      ),
                    ),
                  ])
                : Row(mainAxisSize: MainAxisSize.min, children: [
                    const Icon(Icons.auto_awesome_rounded, color: Colors.white, size: 20),
                    const SizedBox(width: 8),
                    Text(
                      'AI Tạo Lộ Trình 30 Ngày',
                      style: GoogleFonts.outfit(
                        fontWeight: FontWeight.bold,
                        color: Colors.white,
                        fontSize: 15,
                        letterSpacing: 0.2,
                      ),
                    ),
                  ]),
          ),
        ),
      ),
    );
  }
}

// ─── Macro Chip ───────────────────────────────────────────────────────────────
class _MacroChip extends StatelessWidget {
  final String label;
  final Color color;
  const _MacroChip({required this.label, required this.color});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(
        label,
        style: GoogleFonts.inter(
          fontSize: 11,
          fontWeight: FontWeight.w600,
          color: color,
        ),
      ),
    );
  }
}
