import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../core/theme/app_colors.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../../widgets/workout/workout_video_player.dart';
import 'ai_custom_session_screen.dart';
import 'live_workout_screen.dart';
import '../../widgets/anfit_toast.dart';

/// ═══════════════════════════════════════════════════════════
/// WorkoutPlanScreen — Lộ trình tập luyện 30 ngày & Video YouTube
/// Tinh gọn, trực quan, dễ bảo vệ đồ án
/// ═══════════════════════════════════════════════════════════
class WorkoutPlanScreen extends ConsumerStatefulWidget {
  const WorkoutPlanScreen({super.key});

  @override
  ConsumerState<WorkoutPlanScreen> createState() => _WorkoutPlanScreenState();
}

class _WorkoutPlanScreenState extends ConsumerState<WorkoutPlanScreen> {
  List<Map<String, dynamic>> _plans = [];
  bool _loading = true;
  bool _generating = false;
  String? _error;
  int _selectedDayIndex = 0;

  @override
  void initState() {
    super.initState();
    _loadPlans();
  }

  Future<void> _loadPlans() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final todayStr = DateTime.now().toIso8601String().substring(0, 10);
      final data = await ref.read(workoutServiceProvider).getRoadmap(todayStr);
      if (mounted) {
        setState(() {
          _plans = List<Map<String, dynamic>>.from(data);
          _loading = false;
        });
      }
    } on DioException catch (e) {
      if (mounted) setState(() { _loading = false; _error = ApiClient.errorMessage(e); });
    } catch (_) {
      if (mounted) setState(() { _loading = false; _error = 'Không thể tải lộ trình tập.'; });
    }
  }

  Future<void> _generateRoadmap({bool force = false}) async {
    setState(() => _generating = true);
    try {
      await ref.read(workoutServiceProvider).generateRoadmap(force: force);
      if (mounted) AnFitToast.show(context, 'Đã tạo lộ trình 30 ngày thành công!');
      await _loadPlans();
    } on DioException catch (e) {
      final msg = ApiClient.errorMessage(e);
      if (msg == 'ROADMAP_INCOMPLETE') {
        if (mounted) _showIncompleteWarningDialog();
      } else {
        if (mounted) AnFitToast.show(context, msg, isError: true);
      }
    } catch (_) {
      if (mounted) AnFitToast.show(context, 'Lỗi tạo lộ trình, vui lòng thử lại.', isError: true);
    } finally {
      if (mounted) setState(() => _generating = false);
    }
  }

  void _showIncompleteWarningDialog() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Row(
          children: [
            Icon(Icons.warning_amber_rounded, color: Colors.orange),
            SizedBox(width: 8),
            Text('Tạo Lộ Trình Mới?'),
          ],
        ),
        content: const Text('Lộ trình cũ chưa hoàn thành. Tạo mới sẽ lưu tiến độ cũ và khởi tạo lộ trình từ đầu.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Hủy')),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(ctx);
              _generateRoadmap(force: true);
            },
            child: const Text('Tiếp tục tạo'),
          ),
        ],
      ),
    );
  }

  void _showVideoDialog(BuildContext context, String name, String? videoUrl) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(name, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
        content: SizedBox(
          width: double.maxFinite,
          child: WorkoutVideoPlayer(
            videoUrl: (videoUrl != null && videoUrl.isNotEmpty)
                ? videoUrl
                : 'https://www.youtube.com/watch?v=iodQ8pvhb_U',
            height: 220,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Đóng'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final cs = Theme.of(context).colorScheme;

    if (_loading) {
      return Scaffold(
        appBar: AppBar(title: const Text('Lịch Tập Luyện 30 Ngày')),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    if (_error != null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Lịch Tập Luyện 30 Ngày')),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(_error!),
              const SizedBox(height: 12),
              ElevatedButton.icon(
                onPressed: _loadPlans,
                icon: const Icon(Icons.refresh),
                label: const Text('Thử lại'),
              ),
            ],
          ),
        ),
      );
    }

    if (_plans.isEmpty) {
      return Scaffold(
        appBar: AppBar(title: const Text('Lịch Tập Luyện 30 Ngày')),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.fitness_center_outlined, size: 60, color: Colors.grey),
              const SizedBox(height: 16),
              const Text('Chưa có lộ trình tập luyện 30 ngày.'),
              const SizedBox(height: 16),
              ElevatedButton.icon(
                onPressed: _generating ? null : () => _generateRoadmap(),
                icon: _generating
                    ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                    : const Icon(Icons.auto_awesome),
                label: const Text('Tạo lộ trình 30 ngày bằng AI'),
              ),
            ],
          ),
        ),
      );
    }

    final safeIndex = _selectedDayIndex.clamp(0, _plans.length - 1);
    final currentPlan = _plans[safeIndex];
    final activities = (currentPlan['workoutActivities'] as List?)?.cast<Map<String, dynamic>>() ?? [];
    final status = currentPlan['status'] as int? ?? 1; // 1: InProgress, 2: Completed, 3: Skipped
    final estCal = currentPlan['totalEstimatedCaloriesBurn'] ?? 0;
    final estMins = currentPlan['estimatedDurationMinutes'] ?? 45;
    final planName = currentPlan['planName'] ?? 'Buổi tập toàn thân';
    final currentRating = currentPlan['userRating'] as int? ?? 0;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Kế Hoạch Tập Luyện'),
        actions: [
          IconButton(
            tooltip: 'Tùy biến buổi tập AI',
            icon: const Icon(Icons.tune_rounded),
            onPressed: () async {
              final dateStr = currentPlan['planDate']?.toString().substring(0, 10);
              await Navigator.push(context, MaterialPageRoute(builder: (_) => AiCustomSessionScreen(targetDate: dateStr)));
              _loadPlans();
            },
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 100),
        children: [
          // ── THANH CHỌN 30 NGÀY NGANG ──
          SizedBox(
            height: 72,
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              itemCount: _plans.length,
              itemBuilder: (ctx, i) {
                final isSelected = i == safeIndex;
                final p = _plans[i];
                final pStatus = p['status'] as int? ?? 1;
                final isCompleted = pStatus == 2;

                return GestureDetector(
                  onTap: () => setState(() => _selectedDayIndex = i),
                  child: Container(
                    width: 58,
                    margin: const EdgeInsets.only(right: 8),
                    decoration: BoxDecoration(
                      color: isSelected
                          ? cs.primary
                          : (isDark ? AppColors.darkCard : AppColors.lightCard),
                      borderRadius: BorderRadius.circular(14),
                      border: Border.all(
                        color: isSelected ? cs.primary : (isCompleted ? Colors.green : Colors.grey.withValues(alpha: 0.3)),
                        width: isSelected ? 2 : 1,
                      ),
                    ),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text(
                          'N.${i + 1}',
                          style: TextStyle(
                            fontSize: 13,
                            fontWeight: FontWeight.bold,
                            color: isSelected ? Colors.white : (isDark ? Colors.white : Colors.black),
                          ),
                        ),
                        const SizedBox(height: 4),
                        Icon(
                          isCompleted ? Icons.check_circle : Icons.circle_outlined,
                          size: 14,
                          color: isSelected ? Colors.white : (isCompleted ? Colors.green : Colors.grey),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),
          const SizedBox(height: 16),

          // ── CARD THÔNG TIN BUỔI TẬP HIỆN TẠI ──
          Card(
            elevation: 0,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(18),
              side: BorderSide(color: cs.outline.withValues(alpha: 0.15)),
            ),
            child: Padding(
              padding: const EdgeInsets.all(18),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                        decoration: BoxDecoration(
                          color: (status == 2 ? Colors.green : cs.primary).withValues(alpha: 0.15),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(
                          status == 2 ? 'ĐÃ HOÀN THÀNH' : 'HÔM NAY',
                          style: TextStyle(
                            fontSize: 11,
                            fontWeight: FontWeight.bold,
                            color: status == 2 ? Colors.green : cs.primary,
                          ),
                        ),
                      ),
                      const Spacer(),
                      Text('🔥 $estCal kcal  •  ⏱️ $estMins phút', style: TextStyle(color: Colors.grey[600], fontSize: 13)),
                    ],
                  ),
                  const SizedBox(height: 10),
                  Text(planName, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                  const SizedBox(height: 16),

                  // Nút bắt đầu tập Garmin BLE
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton.icon(
                      onPressed: () async {
                        final result = await Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => LiveWorkoutScreen(
                              plan: currentPlan,
                              dayIndex: safeIndex + 1,
                            ),
                          ),
                        );
                        if (result == true) _loadPlans();
                      },
                      icon: const Icon(Icons.play_arrow_rounded, color: Colors.white, size: 24),
                      label: const Text('Bắt Đầu Tập Luyện (Garmin BLE)', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15, color: Colors.white)),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: cs.primary,
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                      ),
                    ),
                  ),
                  const SizedBox(height: 12),

                  // Đánh giá sao độ khó
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text('Đánh giá độ khó: ', style: TextStyle(fontSize: 12, color: Colors.grey[600])),
                      ...List.generate(5, (starIdx) {
                        final star = starIdx + 1;
                        final isFilled = star <= currentRating;
                        return GestureDetector(
                          onTap: () async {
                            try {
                              await ref.read(workoutServiceProvider).rate(currentPlan['id'], rating: star);
                              if (!context.mounted) return;
                              setState(() => currentPlan['userRating'] = star);
                              AnFitToast.show(context, 'Cảm ơn bạn đã đánh giá buổi tập!');
                            } catch (_) {}
                          },
                          child: Icon(
                            isFilled ? Icons.star_rounded : Icons.star_border_rounded,
                            color: isFilled ? Colors.amber : Colors.grey,
                            size: 24,
                          ),
                        );
                      }),
                    ],
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 20),

          // ── DANH SÁCH BÀI TẬP VỚI VIDEO YOUTUBE ──
          Row(
            children: [
              const Text('Danh sách động tác 🏋️', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
              const Spacer(),
              Text('${activities.length} bài tập', style: TextStyle(color: Colors.grey[600], fontSize: 13)),
            ],
          ),
          const SizedBox(height: 10),

          ...activities.map((a) {
            final ex = a['exercise'] as Map<String, dynamic>? ?? {};
            final name = ex['name'] ?? 'Bài tập';
            final muscle = ex['muscleGroup'] ?? 'Toàn thân';
            final sets = a['sets'] ?? 3;
            final reps = a['repsPerSet'] ?? 12;
            final videoUrl = ex['videoUrl'] as String?;

            return Card(
              margin: const EdgeInsets.only(bottom: 10),
              elevation: 0,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(14),
                side: BorderSide(color: cs.outline.withValues(alpha: 0.1)),
              ),
              child: ListTile(
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
                leading: CircleAvatar(
                  backgroundColor: cs.primary.withValues(alpha: 0.1),
                  child: Icon(Icons.fitness_center, color: cs.primary, size: 20),
                ),
                title: Text(name, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                subtitle: Text('$muscle  •  $sets hiệp × $reps lần', style: TextStyle(fontSize: 12, color: Colors.grey[600])),
                trailing: OutlinedButton.icon(
                  onPressed: () => _showVideoDialog(context, name, videoUrl),
                  icon: const Icon(Icons.smart_display_rounded, size: 16, color: Colors.red),
                  label: const Text('Xem Video', style: TextStyle(fontSize: 11)),
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                    side: BorderSide(color: Colors.red.withValues(alpha: 0.4)),
                  ),
                ),
              ),
            );
          }),
        ],
      ),
    );
  }
}
