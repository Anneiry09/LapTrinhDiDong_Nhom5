import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../../widgets/workout/workout_video_player.dart';
import 'ai_custom_session_screen.dart';
import 'live_workout_screen.dart';
import '../../widgets/anfit_toast.dart';

/// ═══════════════════════════════════════════════════════════
/// WorkoutPlanScreen — Premium Lộ Trình 30 Ngày UI
/// ═══════════════════════════════════════════════════════════
class WorkoutPlanScreen extends ConsumerStatefulWidget {
  const WorkoutPlanScreen({super.key});
  @override
  ConsumerState<WorkoutPlanScreen> createState() => _WorkoutPlanScreenState();
}

class _WorkoutPlanScreenState extends ConsumerState<WorkoutPlanScreen>
    with SingleTickerProviderStateMixin {
  List<Map<String, dynamic>> _plans = [];
  bool _loading = true;
  bool _generating = false;
  String? _error;
  late AnimationController _animCtrl;

  @override
  void initState() {
    super.initState();
    _animCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 500),
    );
    _loadPlans();
  }

  @override
  void dispose() {
    _animCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadPlans() async {
    setState(() { _loading = true; _error = null; });
    try {
      final todayStr = DateTime.now().toIso8601String().substring(0, 10);
      final data = await ref.read(workoutServiceProvider).getRoadmap(todayStr);
      setState(() {
        _plans = List<Map<String, dynamic>>.from(data);
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
      await ref.read(workoutServiceProvider).generateRoadmap(force: force);
      if (mounted) {
        _showToast(context, '✅ Đã tạo lộ trình tập luyện 30 ngày thành công!', isSuccess: true);
      }
      await _loadPlans();
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
          'Lộ trình tập luyện 30 ngày hiện tại của bạn chưa hoàn thành 100%. Tạo lộ trình mới sẽ đóng lộ trình cũ (lưu các ngày đã hoàn thành vào lịch sử) và khởi tạo một lộ trình mới bắt đầu từ hôm nay.\n\nBạn có chắc chắn muốn tiếp tục?',
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

  void _showToast(BuildContext context, String message, {required bool isSuccess}) {
    AnFitToast.show(context, message, isError: !isSuccess);
  }

  String _getDailyMuscleSplitTitle(List activities, int dayIndex) {
    if (activities.isEmpty) return 'Nghỉ ngơi & Phục hồi';

    bool hasActive = false;
    for (var act in activities) {
      final name = ((act['exercise']?['name'] ?? '') as String).toLowerCase();
      final mg = ((act['exercise']?['muscleGroup'] ?? '') as String).toLowerCase();
      final isRecovery = name.contains('pose') || name.contains('stretch') || name.contains('yoga') ||
          name.contains('giãn') || name.contains('chào mặt trời') || name.contains('cat-cow') ||
          name.contains('child') || mg.contains('linh hoạt') || mg.contains('giãn cơ');
      if (!isRecovery) {
        hasActive = true;
        break;
      }
    }
    if (!hasActive) return 'Nghỉ ngơi & Phục hồi';

    final Set<String> targetGroups = {};
    for (var act in activities) {
      final name = ((act['exercise']?['name'] ?? '') as String).toLowerCase();
      final mg = ((act['exercise']?['muscleGroup'] ?? '') as String).toLowerCase();
      if (mg.contains('ngực') || mg.contains('chest') || name.contains('push up') || name.contains('press')) targetGroups.add('Ngực');
      if (mg.contains('lưng') || mg.contains('back') || name.contains('pull up') || name.contains('row') || name.contains('pulldown')) targetGroups.add('Lưng');
      if (mg.contains('vai') || mg.contains('shoulder') || name.contains('raise')) targetGroups.add('Vai');
      if (mg.contains('tay sau') || name.contains('tricep') || name.contains('extension')) targetGroups.add('Tay sau');
      if (mg.contains('bắp tay') || mg.contains('bicep') || name.contains('curl')) targetGroups.add('Bắp tay');
      if (mg.contains('chân') || mg.contains('leg') || mg.contains('đùi') || name.contains('squat') || name.contains('lunge')) targetGroups.add('Chân & Đùi');
      if (mg.contains('mông') || mg.contains('glute')) targetGroups.add('Mông');
      if (mg.contains('bụng') || mg.contains('core') || name.contains('plank') || name.contains('crunch')) targetGroups.add('Core');
    }
    return targetGroups.isNotEmpty ? targetGroups.join(' & ') : 'Toàn diện';
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    final todayStr = DateTime.now().toIso8601String().substring(0, 10);

    return Scaffold(
      backgroundColor: isDark ? AppColors.darkBg : AppColors.lightBg,
      body: CustomScrollView(
        slivers: [
          // ── AppBar ────────────────────────────────────────
          SliverAppBar(
            pinned: true,
            backgroundColor: isDark ? AppColors.darkSurface : AppColors.lightSurface,
            surfaceTintColor: Colors.transparent,
            title: Text(
              'Lộ Trình Tập Luyện',
              style: GoogleFonts.outfit(
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: isDark ? AppColors.darkText : AppColors.lightText,
              ),
            ),
            automaticallyImplyLeading: false,
            actions: [
              Padding(
                padding: const EdgeInsets.only(right: 16),
                child: GestureDetector(
                  onTap: () async {
                    final result = await Navigator.push(
                      context,
                      MaterialPageRoute(builder: (_) => const AiCustomSessionScreen()),
                    );
                    if (result == true) _loadPlans();
                  },
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 7),
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [AppColors.auroraPurple, AppColors.auroraCyan],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.auto_awesome_rounded, color: Colors.white, size: 14),
                        const SizedBox(width: 5),
                        Text(
                          'AI Tạo',
                          style: GoogleFonts.inter(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                            fontSize: 12,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),

          // ── Body ──────────────────────────────────────────
          if (_loading)
            SliverFillRemaining(child: _buildSkeleton(isDark))
          else if (_error != null)
            SliverFillRemaining(child: _buildError(isDark))
          else ...[
            SliverPadding(
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 12),
              sliver: _buildRoadmapSliver(isDark, primary, todayStr),
            ),
            SliverToBoxAdapter(
              child: _buildGenerateButton(isDark),
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildSkeleton(bool isDark) {
    final base = isDark ? AppColors.darkCard : AppColors.lightCard;
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        children: List.generate(5, (i) => Container(
          height: 84,
          margin: const EdgeInsets.only(bottom: 12),
          decoration: BoxDecoration(color: base, borderRadius: BorderRadius.circular(20)),
        )),
      ),
    );
  }

  Widget _buildError(bool isDark) {
    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    return Center(
      child: Column(mainAxisSize: MainAxisSize.min, children: [
        Icon(Icons.wifi_off_rounded, size: 52, color: AppColors.error.withValues(alpha: 0.5)),
        const SizedBox(height: 16),
        Text(_error!, style: TextStyle(color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub)),
        const SizedBox(height: 20),
        GestureDetector(
          onTap: _loadPlans,
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

  Widget _buildRoadmapSliver(bool isDark, Color primary, String todayStr) {
    final today = DateTime.now();
    final firstPlanDateStr = _plans.isNotEmpty
        ? _plans.first['planDate']?.toString().substring(0, 10)
        : null;
    final startDate = firstPlanDateStr != null
        ? DateTime.tryParse(firstPlanDateStr) ?? today
        : today;

    final roadmap = List.generate(30, (index) {
      final date = startDate.add(Duration(days: index));
      final dateStr = date.toIso8601String().substring(0, 10);
      final plan = _plans.firstWhere(
        (p) => (p['planDate']?.toString().substring(0, 10) ?? '') == dateStr,
        orElse: () => <String, dynamic>{},
      );
      return {
        'dayIndex': index + 1,
        'date': date,
        'dateStr': dateStr,
        'plan': plan.isNotEmpty ? plan : null,
      };
    });

    return SliverList(
      delegate: SliverChildBuilderDelegate(
        (context, i) {
          final dayData = roadmap[i];
          final dayIndex = dayData['dayIndex'] as int;
          final dateStr = dayData['dateStr'] as String;
          final plan = dayData['plan'] as Map<String, dynamic>?;
          final isToday = dateStr == todayStr;

          final isPast = dateStr.compareTo(todayStr) < 0;
          final status = plan?['status'] as int? ?? 1;
          final completed = status == 2;
          final isMissed = isPast && !completed;

          return AnimatedOpacity(
            opacity: 1.0,
            duration: Duration(milliseconds: 300 + i * 30),
            child: plan != null
                ? _WorkoutDayCard(
                    isDark: isDark,
                    dayIndex: dayIndex,
                    plan: plan,
                    isToday: isToday,
                    isMissed: isMissed,
                    muscleSplit: _getDailyMuscleSplitTitle(
                      plan['workoutActivities'] as List? ?? [],
                      dayIndex,
                    ),
                    onTap: () => _showDayDetail(context, plan, dayIndex),
                  )
                : _RestDayCard(
                    isDark: isDark,
                    dayIndex: dayIndex,
                    dateStr: dateStr,
                    isToday: isToday,
                    onTap: () => _showRestDayBottomSheet(context, dateStr, dayIndex),
                  ),
          );
        },
        childCount: roadmap.length,
      ),
    );
  }

  void _showRestDayBottomSheet(BuildContext context, String dateStr, int dayIndex) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    final textPrimary = isDark ? AppColors.darkText : AppColors.lightText;
    final textSub = isDark ? AppColors.darkTextSub : AppColors.lightTextSub;

    showModalBottomSheet(
      context: context,
      backgroundColor: isDark ? AppColors.darkSurface : AppColors.lightSurface,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      builder: (ctx) => Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 40, height: 4,
              decoration: BoxDecoration(
                color: isDark ? AppColors.darkDivider : AppColors.lightDivider,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            const SizedBox(height: 24),
            Container(
              width: 64, height: 64,
              decoration: BoxDecoration(
                color: primary.withValues(alpha: 0.1),
                shape: BoxShape.circle,
              ),
              child: Icon(Icons.spa_rounded, color: primary, size: 32),
            ),
            const SizedBox(height: 16),
            Text(
              'Ngày $dayIndex — Nghỉ & Phục Hồi',
              style: GoogleFonts.outfit(fontSize: 18, fontWeight: FontWeight.bold, color: textPrimary),
            ),
            const SizedBox(height: 6),
            Text(
              dateStr,
              style: GoogleFonts.inter(fontSize: 13, color: textSub),
            ),
            const SizedBox(height: 16),
            Text(
              'Để cơ thể nghỉ ngơi và phục hồi giúp cơ bắp phát triển tốt hơn. Uống đủ nước và duy trì chế độ dinh dưỡng lành mạnh nhé!',
              textAlign: TextAlign.center,
              style: GoogleFonts.inter(fontSize: 14, color: textSub, height: 1.6),
            ),
            const SizedBox(height: 28),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () async {
                  Navigator.pop(ctx);
                  final result = await Navigator.push(
                    context,
                    MaterialPageRoute(builder: (_) => AiCustomSessionScreen(targetDate: dateStr)),
                  );
                  if (result == true) _loadPlans();
                },
                icon: const Icon(Icons.auto_awesome_rounded, size: 18),
                label: const Text('Tạo buổi tập AI cho ngày này'),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 15),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                ),
              ),
            ),
            const SizedBox(height: 10),
            SizedBox(
              width: double.infinity,
              child: OutlinedButton(
                onPressed: () => Navigator.pop(ctx),
                style: OutlinedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 15),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                ),
                child: const Text('Đóng'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _showDayDetail(BuildContext context, Map<String, dynamic> plan, int dayIndex) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    final textPrimary = isDark ? AppColors.darkText : AppColors.lightText;
    final textSub = isDark ? AppColors.darkTextSub : AppColors.lightTextSub;
    final surfaceBg = isDark ? AppColors.darkSurface : AppColors.lightSurface;
    final activities = plan['workoutActivities'] as List? ?? [];
    final estCal = plan['totalEstimatedCaloriesBurn'] ?? 0;
    final planDate = plan['planDate']?.toString().substring(0, 10) ?? '';
    final planId = plan['id'];
    final status = plan['status'] as int? ?? 1;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: surfaceBg,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      builder: (ctx) => DraggableScrollableSheet(
        expand: false,
        initialChildSize: 0.7,
        maxChildSize: 0.95,
        builder: (_, scrollCtrl) => ListView(
          controller: scrollCtrl,
          padding: const EdgeInsets.all(24),
          children: [
            // Drag handle
            Center(
              child: Container(
                width: 40, height: 4,
                decoration: BoxDecoration(
                  color: isDark ? AppColors.darkDivider : AppColors.lightDivider,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
            ),
            const SizedBox(height: 20),
            // Header
            Row(
              children: [
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(
                      'Ngày $dayIndex — Chi Tiết',
                      style: GoogleFonts.outfit(fontSize: 20, fontWeight: FontWeight.bold, color: textPrimary),
                    ),
                    Text(planDate, style: GoogleFonts.inter(fontSize: 13, color: textSub)),
                  ]),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  decoration: BoxDecoration(
                    color: status == 2
                        ? AppColors.success.withValues(alpha: 0.12)
                        : primary.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Text(
                    status == 2 ? '✓ Hoàn thành' : 'Chưa tập',
                    style: GoogleFonts.inter(
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                      color: status == 2 ? AppColors.success : primary,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Text(
              '${activities.length} bài tập  •  $estCal kcal',
              style: GoogleFonts.inter(fontSize: 13, color: textSub),
            ),
            const SizedBox(height: 16),

            // ── Rating Section ──
            StatefulBuilder(
              builder: (sheetCtx, setSheetState) {
                final currentRating = plan['userRating'] as int? ?? 0;
                return Container(
                  margin: const EdgeInsets.only(bottom: 20),
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                  decoration: BoxDecoration(
                    color: isDark ? AppColors.darkCard : AppColors.lightCard,
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(
                      color: primary.withValues(alpha: 0.15),
                      width: 1,
                    ),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: isDark ? 0.1 : 0.03),
                        blurRadius: 10,
                        offset: const Offset(0, 2),
                      ),
                    ],
                  ),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Icon(Icons.auto_awesome_rounded, color: Colors.amber, size: 16),
                          const SizedBox(width: 8),
                          Text(
                            'Đánh giá buổi tập AI hôm nay',
                            style: GoogleFonts.outfit(
                              fontWeight: FontWeight.bold,
                              fontSize: 14,
                              color: textPrimary,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Đánh giá của bạn giúp AI cải thiện độ khó và bài tập tốt hơn!',
                        style: GoogleFonts.inter(
                          color: textSub,
                          fontSize: 11,
                        ),
                        textAlign: TextAlign.center,
                      ),
                      const SizedBox(height: 12),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: List.generate(5, (index) {
                          final starValue = index + 1;
                          final isFilled = starValue <= currentRating;
                          return GestureDetector(
                            onTap: () async {
                              setSheetState(() {
                                plan['userRating'] = starValue;
                              });
                              setState(() {}); // Đồng bộ màn hình chính
                              try {
                                await ref.read(workoutServiceProvider).rate(planId, rating: starValue);
                                if (ctx.mounted) {
                                  if (starValue <= 3) {
                                    AnFitToast.show(
                                      ctx,
                                      'Cảm ơn góp ý! Ban quản trị sẽ rà soát độ khó bài tập này. 🦾',
                                    );
                                  } else {
                                    AnFitToast.show(
                                      ctx,
                                      'Tuyệt vời! Hãy tiếp tục duy trì phong độ tập luyện nhé! 🔥',
                                    );
                                  }
                                }
                              } on DioException catch (e) {
                                if (ctx.mounted) {
                                  AnFitToast.show(ctx, ApiClient.errorMessage(e), isError: true);
                                }
                              } catch (e) {
                                if (ctx.mounted) {
                                  AnFitToast.show(ctx, 'Lỗi gửi đánh giá, vui lòng thử lại.', isError: true);
                                }
                              }
                            },
                            child: Padding(
                              padding: const EdgeInsets.symmetric(horizontal: 6),
                              child: Icon(
                                isFilled ? Icons.star_rounded : Icons.star_outline_rounded,
                                color: isFilled ? Colors.amber : textSub.withValues(alpha: 0.3),
                                size: 32,
                              ),
                            ),
                          );
                        }),
                      ),
                    ],
                  ),
                );
              },
            ),

            const SizedBox(height: 12),

            // Activities list
            ...List.generate(activities.length, (i) {
              final a = activities[i];
              final ex = a['exercise'] as Map<String, dynamic>? ?? {};
              final name = ex['name'] ?? 'Bài tập';
              final muscle = ex['muscleGroup'] ?? '';
              final sets = a['sets'] ?? a['setsCount'] ?? 0;
              final reps = a['repsPerSet'] ?? a['reps'] ?? 0;
              final rest = a['restTimeSeconds'] ?? 0;
              final dur = a['durationMinutes'] ?? 8;
              final phase = a['phase'] as String? ?? 'Core Workout';
              final isWarmup = phase == 'Warm-up';
              final isCooldown = phase == 'Cool-down';
              final isSpecial = isWarmup || isCooldown;

              final accentColor = isWarmup
                  ? AppColors.metricSteps
                  : isCooldown
                      ? AppColors.metricSleep
                      : primary;

              return Container(
                margin: const EdgeInsets.only(bottom: 10),
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(16),
                ),
                clipBehavior: Clip.antiAlias,
                child: Material(
                  color: isSpecial
                      ? accentColor.withValues(alpha: isDark ? 0.08 : 0.05)
                      : (isDark ? AppColors.darkCard : AppColors.lightCard),
                  child: InkWell(
                    onTap: () {
                      _showExerciseDetailSheet(context, ex);
                    },
                    child: Container(
                      padding: const EdgeInsets.all(14),
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(16),
                        border: Border.all(
                          color: isSpecial
                              ? accentColor.withValues(alpha: 0.2)
                              : (isDark
                                  ? Colors.white.withValues(alpha: 0.06)
                                  : Colors.black.withValues(alpha: 0.05)),
                          width: 0.8,
                        ),
                      ),
                      child: Row(children: [
                        Container(
                          width: 36, height: 36,
                          decoration: BoxDecoration(
                            color: accentColor.withValues(alpha: 0.12),
                            borderRadius: BorderRadius.circular(10),
                          ),
                          child: isSpecial
                              ? Icon(
                                  isWarmup ? Icons.wb_sunny_rounded : Icons.spa_rounded,
                                  size: 18,
                                  color: accentColor,
                                )
                              : Center(
                                  child: Text(
                                    '${i + 1}',
                                    style: GoogleFonts.outfit(
                                      fontWeight: FontWeight.bold,
                                      color: accentColor,
                                      fontSize: 14,
                                    ),
                                  ),
                                ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                            Text(
                              name as String,
                              style: GoogleFonts.outfit(
                                  fontWeight: FontWeight.w600,
                                  fontSize: 14,
                                  color: textPrimary,
                                ),
                            ),
                            const SizedBox(height: 3),
                            Text(
                              isSpecial
                                  ? (isWarmup ? 'Khởi động • 10 phút' : 'Giãn cơ • 10 phút')
                                  : '$sets hiệp × $reps lần  •  Nghỉ ${rest}s${muscle.isNotEmpty ? '  •  $muscle' : ''}',
                              style: GoogleFonts.inter(fontSize: 11, color: textSub),
                            ),
                          ]),
                        ),
                        if (dur > 0)
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                            decoration: BoxDecoration(
                              color: isDark
                                  ? Colors.white.withValues(alpha: 0.06)
                                  : Colors.black.withValues(alpha: 0.04),
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Text(
                              '${dur}ph',
                              style: GoogleFonts.inter(fontSize: 11, color: textSub, fontWeight: FontWeight.w500),
                            ),
                          ),
                      ]),
                    ),
                  ),
                ),
              );
            }),

            const SizedBox(height: 20),
            if (status != 2) ...[
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed: () async {
                    final planDate = plan['planDate']?.toString().substring(0, 10) ?? '';
                    final todayStr = DateTime.now().toIso8601String().substring(0, 10);
                    final isFuture = planDate.compareTo(todayStr) > 0;

                    if (isFuture) {
                      _showToast(context, '🍿 Hãy kiên nhẫn một chút nhé! Ngày tập này thuộc về tương lai. Hẹn gặp bạn vào ngày $planDate để cùng nhau tập luyện nha! 🦾', isSuccess: false);
                      return;
                    }

                    Navigator.pop(ctx); // Close bottom sheet
                    final result = await Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => LiveWorkoutScreen(
                          plan: plan,
                          dayIndex: dayIndex,
                        ),
                      ),
                    );
                    if (result == true) _loadPlans();
                  },
                  icon: const Icon(Icons.play_circle_fill_rounded, size: 18),
                  label: const Text('Bắt đầu Luyện tập (Garmin BLE)'),
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 15),
                    backgroundColor: primary,
                    foregroundColor: isDark ? AppColors.darkBg : Colors.white,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                    elevation: 3,
                  ),
                ),
              ),
              const SizedBox(height: 10),
              SizedBox(
                width: double.infinity,
                child: OutlinedButton.icon(
                  onPressed: () async {
                    final planDate = plan['planDate']?.toString().substring(0, 10) ?? '';
                    final todayStr = DateTime.now().toIso8601String().substring(0, 10);
                    final isPast = planDate.compareTo(todayStr) < 0;
                    final isMissed = isPast && status != 2;
                    final isFuture = planDate.compareTo(todayStr) > 0;

                    if (isFuture) {
                      _showToast(context, '🍿 Hãy kiên nhẫn một chút nhé! Ngày tập này thuộc về tương lai. Hẹn gặp bạn vào ngày $planDate để cùng nhau tập luyện nha! 🦾', isSuccess: false);
                      return;
                    }

                    if (isMissed) {
                      _showToast(context, '⚠️ Không thể hoàn thành lịch tập đã bị bỏ lỡ!', isSuccess: false);
                      return;
                    }

                    try {
                      await ref.read(workoutServiceProvider).updateStatus(planId, status: 2);
                      if (ctx.mounted) Navigator.pop(ctx);
                      _loadPlans();
                    } catch (_) {}
                  },
                  icon: const Icon(Icons.check_circle_outline_rounded, size: 18),
                  label: const Text('Đánh dấu hoàn thành thủ công'),
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  ),
                ),
              ),
            ],
            const SizedBox(height: 12),
          ],
        ),
      ),
    );
  }

  void _showExerciseDetailSheet(BuildContext context, Map<String, dynamic> exercise) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final textPrimary = isDark ? AppColors.darkText : AppColors.lightText;
    final surfaceBg = isDark ? AppColors.darkSurface : AppColors.lightSurface;
    
    final name = exercise['name'] ?? 'Bài tập';
    final muscle = exercise['muscleGroup'] ?? 'Toàn thân';
    final videoUrl = exercise['instructionUrl'] as String? ?? '';
    final calories = exercise['baseCaloriesBurned'] ?? 0;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: surfaceBg,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      builder: (ctx) {
        return DraggableScrollableSheet(
          initialChildSize: 0.8,
          minChildSize: 0.5,
          maxChildSize: 0.95,
          expand: false,
          builder: (_, scrollController) {
            return SingleChildScrollView(
              controller: scrollController,
              padding: const EdgeInsets.all(24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Drag handle
                  Center(
                    child: Container(
                      width: 40, height: 4,
                      decoration: BoxDecoration(
                        color: isDark ? AppColors.darkDivider : AppColors.lightDivider,
                        borderRadius: BorderRadius.circular(2),
                      ),
                    ),
                  ),
                  const SizedBox(height: 20),
                  
                  // Exercise Name & Muscle Group
                  Text(
                    name,
                    style: GoogleFonts.outfit(
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                      color: textPrimary,
                    ),
                  ),
                  const SizedBox(height: 6),
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                        decoration: BoxDecoration(
                          color: AppColors.auroraCyan.withValues(alpha: 0.12),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(
                          'Cơ tác động: $muscle',
                          style: GoogleFonts.inter(
                            fontSize: 11,
                            fontWeight: FontWeight.bold,
                            color: AppColors.auroraCyan,
                          ),
                        ),
                      ),
                      const SizedBox(width: 10),
                      if (calories > 0)
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                          decoration: BoxDecoration(
                            color: AppColors.darkCoral.withValues(alpha: 0.12),
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: Text(
                            '~$calories kcal/phút',
                            style: GoogleFonts.inter(
                              fontSize: 11,
                              fontWeight: FontWeight.bold,
                              color: AppColors.darkCoral,
                            ),
                          ),
                        ),
                    ],
                  ),
                  const SizedBox(height: 24),

                  // ── VIDEO PLAYER / Fallback Placeholder ──
                  if (videoUrl.isNotEmpty) ...[
                    WorkoutVideoPlayer(videoUrl: videoUrl),
                    const SizedBox(height: 24),
                  ] else ...[
                    Container(
                      height: 220,
                      width: double.infinity,
                      decoration: BoxDecoration(
                        color: isDark ? AppColors.darkCard : AppColors.lightCard,
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: const Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(Icons.video_library_rounded, size: 48, color: Colors.grey),
                            SizedBox(height: 12),
                            Text(
                              'Không tìm thấy video hướng dẫn cho động tác này.',
                              style: TextStyle(color: Colors.grey, fontSize: 13),
                            ),
                          ],
                        ),
                      ),
                    ),
                    const SizedBox(height: 24),
                  ],

                  // Description / Technical steps
                  Text(
                    'Hướng Dẫn Kỹ Thuật',
                    style: GoogleFonts.outfit(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: textPrimary,
                    ),
                  ),
                  const SizedBox(height: 12),
                  Text(
                    'Hãy thực hiện đúng tư thế theo video hướng dẫn ở trên. Đảm bảo khởi động kỹ trước khi tập để tránh các chấn thương không đáng có và đạt hiệu quả tối đa.',
                    style: GoogleFonts.inter(
                      fontSize: 13,
                      color: isDark ? AppColors.darkTextSub : AppColors.lightTextSub,
                      height: 1.6,
                    ),
                  ),
                  const SizedBox(height: 30),
                  
                  // Close button
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton(
                      onPressed: () => Navigator.pop(ctx),
                      style: OutlinedButton.styleFrom(
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                      ),
                      child: const Text('Đóng'),
                    ),
                  ),
                ],
              ),
            );
          },
        );
      },
    );
  }

  Widget _buildGenerateButton(bool isDark) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 120),
      child: GestureDetector(
        onTap: _generating ? null : () => _generateRoadmap(),
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
          height: 54,
          decoration: BoxDecoration(
            gradient: _generating
                ? null
                : const LinearGradient(
                    colors: [AppColors.auroraPurple, AppColors.auroraCyan],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
            color: _generating ? (isDark ? AppColors.darkCard : AppColors.lightCard) : null,
            borderRadius: BorderRadius.circular(18),
            boxShadow: _generating
                ? null
                : [
                    BoxShadow(
                      color: AppColors.auroraPurple.withValues(alpha: 0.3),
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
                      'AI Tạo Lộ Trình 30 Ngày Mới',
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

// ─── Workout Day Card ─────────────────────────────────────────────────────────
class _WorkoutDayCard extends StatelessWidget {
  final bool isDark;
  final int dayIndex;
  final Map<String, dynamic> plan;
  final bool isToday;
  final bool isMissed;
  final String muscleSplit;
  final VoidCallback onTap;

  const _WorkoutDayCard({
    required this.isDark,
    required this.dayIndex,
    required this.plan,
    required this.isToday,
    required this.isMissed,
    required this.muscleSplit,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    final textPrimary = isDark ? AppColors.darkText : AppColors.lightText;
    final textSub = isDark ? AppColors.darkTextSub : AppColors.lightTextSub;
    final cardBg = isDark ? AppColors.darkCard : AppColors.lightSurface;
    final status = plan['status'] as int? ?? 1;
    final completed = status == 2;
    final activities = plan['workoutActivities'] as List? ?? [];
    final estCal = plan['totalEstimatedCaloriesBurn'] ?? 0;
    final planDate = plan['planDate']?.toString().substring(0, 10) ?? '';

    return GestureDetector(
      onTap: onTap,
      child: Container(
        margin: const EdgeInsets.only(bottom: 10),
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: isToday ? primary.withValues(alpha: 0.06) : cardBg,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: isToday
                ? primary.withValues(alpha: 0.4)
                : completed
                    ? AppColors.success.withValues(alpha: 0.3)
                    : isMissed
                        ? AppColors.error.withValues(alpha: 0.3)
                        : (isDark ? Colors.white.withValues(alpha: 0.07) : Colors.black.withValues(alpha: 0.05)),
            width: isToday ? 1.5 : 0.8,
          ),
          boxShadow: isToday
              ? [BoxShadow(color: primary.withValues(alpha: 0.12), blurRadius: 12, offset: const Offset(0, 4))]
              : null,
        ),
        child: Row(children: [
          // Day badge
          Container(
            width: 48, height: 48,
            decoration: BoxDecoration(
              color: completed
                  ? AppColors.success.withValues(alpha: 0.12)
                  : isMissed
                      ? AppColors.error.withValues(alpha: 0.12)
                      : (isToday ? primary.withValues(alpha: 0.15) : primary.withValues(alpha: 0.08)),
              borderRadius: BorderRadius.circular(14),
            ),
            child: completed
                ? const Icon(Icons.check_rounded, color: AppColors.success, size: 22)
                : isMissed
                    ? const Icon(Icons.close_rounded, color: AppColors.error, size: 22)
                    : Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            'D$dayIndex',
                            style: GoogleFonts.outfit(
                              fontWeight: FontWeight.bold,
                              color: primary,
                              fontSize: 13,
                            ),
                          ),
                        ],
                      ),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Row(children: [
                Flexible(
                  child: Text(
                    muscleSplit,
                    style: GoogleFonts.outfit(
                      fontWeight: FontWeight.w600,
                      fontSize: 14,
                      color: textPrimary,
                    ),
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                if (isToday) ...[
                  const SizedBox(width: 8),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 2),
                    decoration: BoxDecoration(
                      color: primary,
                      borderRadius: BorderRadius.circular(6),
                    ),
                    child: Text(
                      'HÔM NAY',
                      style: GoogleFonts.inter(
                        fontSize: 9,
                        color: isDark ? AppColors.darkBg : Colors.white,
                        fontWeight: FontWeight.bold,
                        letterSpacing: 0.5,
                      ),
                    ),
                  ),
                ] else if (isMissed) ...[
                  const SizedBox(width: 8),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 2),
                    decoration: BoxDecoration(
                      color: AppColors.error,
                      borderRadius: BorderRadius.circular(6),
                    ),
                    child: Text(
                      'BỎ LỠ',
                      style: GoogleFonts.inter(
                        fontSize: 9,
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                        letterSpacing: 0.5,
                      ),
                    ),
                  ),
                ],
              ]),
              const SizedBox(height: 4),
              Text(
                '${activities.length} bài  •  $estCal kcal  •  $planDate',
                style: GoogleFonts.inter(fontSize: 11, color: textSub),
              ),
            ]),
          ),
          completed
              ? const Icon(Icons.check_circle_rounded, color: AppColors.success, size: 22)
              : isMissed
                  ? const Icon(Icons.error_outline_rounded, color: AppColors.error, size: 20)
                  : Icon(Icons.chevron_right_rounded, color: textSub.withValues(alpha: 0.6), size: 20),
        ]),
      ),
    );
  }
}

// ─── Rest Day Card ────────────────────────────────────────────────────────────
class _RestDayCard extends StatelessWidget {
  final bool isDark;
  final int dayIndex;
  final String dateStr;
  final bool isToday;
  final VoidCallback onTap;

  const _RestDayCard({
    required this.isDark,
    required this.dayIndex,
    required this.dateStr,
    required this.isToday,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final primary = isDark ? AppColors.darkPrimary : AppColors.lightPrimary;
    final textPrimary = isDark ? AppColors.darkText : AppColors.lightText;
    final textSub = isDark ? AppColors.darkTextSub : AppColors.lightTextSub;

    return GestureDetector(
      onTap: onTap,
      child: Container(
        margin: const EdgeInsets.only(bottom: 10),
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: isDark
              ? Colors.white.withValues(alpha: 0.02)
              : Colors.black.withValues(alpha: 0.015),
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: isToday
                ? primary.withValues(alpha: 0.3)
                : (isDark ? Colors.white.withValues(alpha: 0.05) : Colors.black.withValues(alpha: 0.04)),
            width: 0.8,
          ),
        ),
        child: Row(children: [
          Container(
            width: 48, height: 48,
            decoration: BoxDecoration(
              color: isDark
                  ? Colors.white.withValues(alpha: 0.04)
                  : Colors.black.withValues(alpha: 0.03),
              borderRadius: BorderRadius.circular(14),
            ),
            child: Icon(
              Icons.spa_rounded,
              color: isDark
                  ? AppColors.darkTextSub.withValues(alpha: 0.5)
                  : AppColors.lightTextSub.withValues(alpha: 0.5),
              size: 20,
            ),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Row(children: [
                Text(
                  'Ngày $dayIndex: Nghỉ & Phục hồi',
                  style: GoogleFonts.outfit(
                    fontWeight: FontWeight.w500,
                    fontSize: 13,
                    color: textPrimary.withValues(alpha: 0.5),
                  ),
                ),
                if (isToday) ...[
                  const SizedBox(width: 8),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 2),
                    decoration: BoxDecoration(
                      color: isDark ? AppColors.darkDivider : AppColors.lightDivider,
                      borderRadius: BorderRadius.circular(6),
                    ),
                    child: Text(
                      'HÔM NAY',
                      style: GoogleFonts.inter(
                        fontSize: 9,
                        color: textSub,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
              ]),
              const SizedBox(height: 3),
              Text(
                'Nhấn để tạo lịch tập bằng AI  •  $dateStr',
                style: GoogleFonts.inter(fontSize: 11, color: textSub.withValues(alpha: 0.6)),
              ),
            ]),
          ),
          Icon(Icons.add_circle_outline_rounded, color: primary.withValues(alpha: 0.5), size: 20),
        ]),
      ),
    );
  }
}
