import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../core/theme/app_colors.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import 'meal_plan_screen.dart';
import '../../widgets/anfit_toast.dart';

/// ═══════════════════════════════════════════════════════════
/// MealRoadmapScreen — Lộ Trình Dinh Dưỡng 30 Ngày
/// Tinh gọn, trực quan, dễ bảo vệ đồ án
/// ═══════════════════════════════════════════════════════════
class MealRoadmapScreen extends ConsumerStatefulWidget {
  const MealRoadmapScreen({super.key});

  @override
  ConsumerState<MealRoadmapScreen> createState() => _MealRoadmapScreenState();
}

class _MealRoadmapScreenState extends ConsumerState<MealRoadmapScreen> {
  bool _loading = true;
  bool _generating = false;
  String? _error;
  List<Map<String, dynamic>> _plans = [];

  @override
  void initState() {
    super.initState();
    _loadRoadmap();
  }

  Future<void> _loadRoadmap() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final data = await ref.read(nutritionServiceProvider).getRoadmap();
      if (mounted) {
        setState(() {
          _plans = data.cast<Map<String, dynamic>>();
          _loading = false;
        });
      }
    } on DioException catch (e) {
      if (mounted) setState(() { _loading = false; _error = ApiClient.errorMessage(e); });
    } catch (_) {
      if (mounted) setState(() { _loading = false; _error = 'Không thể tải lộ trình dinh dưỡng.'; });
    }
  }

  Future<void> _generateRoadmap({bool force = false}) async {
    setState(() => _generating = true);
    try {
      await ref.read(nutritionServiceProvider).generateRoadmap(force: force);
      if (mounted) AnFitToast.show(context, 'Đã tạo lộ trình 30 ngày thành công!');
      await _loadRoadmap();
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
            Text('Lộ Trình Chưa Xong!'),
          ],
        ),
        content: const Text('Lộ trình hiện tại chưa hoàn thành 100%. Tạo mới sẽ kết thúc lộ trình cũ và bắt đầu lộ trình mới từ hôm nay.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Hủy')),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(ctx);
              _generateRoadmap(force: true);
            },
            child: const Text('Tiếp tục tạo mới'),
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
    ).then((_) => _loadRoadmap());
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final isDark = Theme.of(context).brightness == Brightness.dark;

    if (_loading) {
      return Scaffold(
        appBar: AppBar(title: const Text('Lộ Trình Dinh Dưỡng 30 Ngày')),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    if (_error != null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Lộ Trình Dinh Dưỡng 30 Ngày')),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(_error!),
              const SizedBox(height: 12),
              ElevatedButton.icon(
                onPressed: _loadRoadmap,
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
        appBar: AppBar(title: const Text('Lộ Trình Dinh Dưỡng 30 Ngày')),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.restaurant_menu, size: 60, color: Colors.grey),
              const SizedBox(height: 16),
              const Text('Chưa có lộ trình dinh dưỡng 30 ngày.'),
              const SizedBox(height: 16),
              ElevatedButton.icon(
                onPressed: _generating ? null : () => _generateRoadmap(),
                icon: _generating
                    ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                    : const Icon(Icons.auto_awesome),
                label: const Text('Tạo lộ trình dinh dưỡng AI'),
              ),
            ],
          ),
        ),
      );
    }

    final completedCount = _plans.where((p) => (p['status'] as int? ?? 1) == 2).length;
    final progressPercent = (_plans.isNotEmpty ? completedCount / _plans.length : 0.0);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Dinh Dưỡng 30 Ngày'),
        actions: [
          IconButton(
            tooltip: 'Tạo mới lộ trình AI',
            icon: const Icon(Icons.auto_awesome),
            onPressed: _generating ? null : () => _generateRoadmap(),
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 100),
        children: [
          // ── PROGRESS HEADER ──
          Card(
            elevation: 0,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
              side: BorderSide(color: cs.outline.withValues(alpha: 0.15)),
            ),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Text(
                        'Tiến độ 30 ngày: $completedCount/30 ngày',
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                      ),
                      const Spacer(),
                      Text('${(progressPercent * 100).toInt()}%', style: TextStyle(fontWeight: FontWeight.bold, color: cs.primary)),
                    ],
                  ),
                  const SizedBox(height: 10),
                  ClipRRect(
                    borderRadius: BorderRadius.circular(6),
                    child: LinearProgressIndicator(
                      value: progressPercent,
                      minHeight: 8,
                      backgroundColor: cs.primary.withValues(alpha: 0.15),
                      valueColor: AlwaysStoppedAnimation<Color>(cs.primary),
                    ),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),

          // ── GRID 30 NGÀY ──
          const Text('Danh sách ngày ăn 🥗', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          const SizedBox(height: 10),

          GridView.builder(
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            itemCount: _plans.length,
            gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: 5,
              crossAxisSpacing: 8,
              mainAxisSpacing: 8,
              childAspectRatio: 0.9,
            ),
            itemBuilder: (ctx, i) {
              final plan = _plans[i];
              final status = plan['status'] as int? ?? 1; // 1: InProgress, 2: Completed
              final isCompleted = status == 2;
              final calo = (plan['totalCalories'] as num?)?.round() ?? 0;

              return InkWell(
                onTap: () => _openDayDetail(i),
                borderRadius: BorderRadius.circular(12),
                child: Container(
                  decoration: BoxDecoration(
                    color: isCompleted
                        ? Colors.green.withValues(alpha: 0.12)
                        : (isDark ? AppColors.darkCard : AppColors.lightCard),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(
                      color: isCompleted ? Colors.green : Colors.grey.withValues(alpha: 0.25),
                      width: isCompleted ? 1.5 : 1,
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
                          color: isCompleted ? Colors.green : null,
                        ),
                      ),
                      const SizedBox(height: 2),
                      Icon(
                        isCompleted ? Icons.check_circle : Icons.restaurant,
                        size: 14,
                        color: isCompleted ? Colors.green : Colors.grey,
                      ),
                      if (calo > 0) ...[
                        const SizedBox(height: 2),
                        Text('$calo', style: const TextStyle(fontSize: 9.5, color: Colors.grey)),
                      ],
                    ],
                  ),
                ),
              );
            },
          ),
        ],
      ),
    );
  }
}
