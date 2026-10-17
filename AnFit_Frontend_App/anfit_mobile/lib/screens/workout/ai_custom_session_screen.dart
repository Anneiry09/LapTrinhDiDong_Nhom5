import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../../widgets/anfit_toast.dart';

/// Hybrid AI — Tạo Buổi Tập On-Demand
class AiCustomSessionScreen extends ConsumerStatefulWidget {
  final String? targetDate;
  const AiCustomSessionScreen({super.key, this.targetDate});
  @override
  ConsumerState<AiCustomSessionScreen> createState() => _State();
}

class _State extends ConsumerState<AiCustomSessionScreen> {
  int _duration = 45;
  String _workoutType = '';
  String _level = '';
  bool _includeWarmup = true;
  bool _includeCooldown = true;
  bool _isGenerating = false;
  List<Map<String, dynamic>> _result = [];
  int _totalDuration = 0;
  int _totalCalories = 0;
  final List<String> _muscleOptions = ['Ngực', 'Lưng', 'Vai', 'Tay', 'Chân', 'Bụng'];
  final List<String> _selectedMuscles = [];

  final _types = [
    (Icons.fitness_center, 'Gym / Tạ', 4),
    (Icons.directions_run, 'Cardio / Chạy bộ', 2),
    (Icons.timer, 'HIIT', 1),
    (Icons.self_improvement, 'Yoga', 5),
    (Icons.home, 'Tập tại nhà', 1),
  ];
  final _levels = [('Người mới', 1), ('Trung bình', 2), ('Nâng cao', 3)];

  int get _workoutTypeId {
    final idx = _types.indexWhere((t) => t.$2 == _workoutType);
    return idx >= 0 ? _types[idx].$3 : 1;
  }

  int get _levelId {
    final idx = _levels.indexWhere((l) => l.$1 == _level);
    return idx >= 0 ? _levels[idx].$2 : 1;
  }

  void _generate() async {
    setState(() { _isGenerating = true; _result = []; });

    try {
      final planDate = widget.targetDate ?? DateTime.now().toIso8601String().substring(0, 10);
      final data = await ref.read(workoutServiceProvider).generatePlan(
        planDate: planDate,
        maxMinutes: _duration,
        difficulty: _levelId,
        workoutType: _workoutTypeId,
        targetMuscleGroups: _selectedMuscles.isNotEmpty ? _selectedMuscles : null,
        hasWarmup: _includeWarmup,
        hasCooldown: _includeCooldown,
      );

      if (mounted) {
        final activities = data['workoutActivities'] as List? ?? [];
        final estCal = (data['totalEstimatedCaloriesBurn'] ?? 0);
        setState(() {
          _isGenerating = false;
          _totalDuration = _duration;
          _totalCalories = (estCal is num) ? estCal.round() : 0;
          _result = activities.map<Map<String, dynamic>>((a) {
            final ex = a['exercise'] as Map<String, dynamic>? ?? {};
            return {
              'phase': a['phase'] ?? 'Core',
              'name': ex['name'] ?? 'Bài tập',
              'durationMins': a['durationMinutes'] ?? 8,
              'caloriesBurned': a['estimatedCaloriesBurned'] ?? 0,
              'sets': a['sets'] ?? 0,
              'reps': a['repsPerSet'] ?? 0,
              'restTimeSeconds': a['restTimeSeconds'] ?? 0,
              'muscle': ex['muscleGroup'] ?? '',
            };
          }).toList();
        });
      }
    } on DioException catch (e) {
      if (mounted) {
        setState(() => _isGenerating = false);
        AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isGenerating = false);
        AnFitToast.show(context, 'Lỗi tạo buổi tập, thử lại.', isError: true);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, result) {
        if (didPop) return;
        Navigator.pop(context, _result.isNotEmpty);
      },
      child: Scaffold(
        appBar: AppBar(
          title: const Text('🤖 AI Tạo Buổi Tập'),
          leading: IconButton(
            icon: const Icon(Icons.arrow_back),
            onPressed: () {
              Navigator.pop(context, _result.isNotEmpty);
            },
          ),
        ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          // Duration
          Text('⏱ Thời lượng tối đa', style: textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(color: cs.surface, borderRadius: BorderRadius.circular(16), border: Border.all(color: cs.outline.withValues(alpha: 0.1))),
            child: Column(children: [
              Text('$_duration phút', style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: cs.primary)),
              Slider(value: _duration.toDouble(), min: 15, max: 90, divisions: 5, activeColor: cs.primary, onChanged: (v) => setState(() => _duration = v.round())),
            ]),
          ),
          const SizedBox(height: 20),

          // Workout Type
          Text('🏋️ Thể loại bài tập', style: textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          Wrap(spacing: 8, runSpacing: 8, children: _types.map((t) {
            final selected = _workoutType == t.$2;
            return ChoiceChip(
              selected: selected,
              onSelected: (_) => setState(() => _workoutType = t.$2),
              avatar: Icon(t.$1, size: 18, color: selected ? cs.onPrimary : cs.primary),
              label: Text(t.$2),
              labelStyle: TextStyle(color: selected ? cs.onPrimary : cs.onSurface),
              selectedColor: cs.primary, backgroundColor: cs.surface,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            );
          }).toList()),
          const SizedBox(height: 20),

          // Target Muscle Groups Multi-Select
          Text('🎯 Nhóm cơ muốn tập (Chọn nhiều)', style: textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          Wrap(spacing: 8, runSpacing: 8, children: _muscleOptions.map((m) {
            final selected = _selectedMuscles.contains(m);
            return FilterChip(
              selected: selected,
              onSelected: (val) {
                setState(() {
                  if (val) {
                    _selectedMuscles.add(m);
                  } else {
                    _selectedMuscles.remove(m);
                  }
                });
              },
              label: Text(m),
              labelStyle: TextStyle(color: selected ? cs.onPrimary : cs.onSurface, fontWeight: selected ? FontWeight.bold : FontWeight.normal),
              selectedColor: cs.primary, backgroundColor: cs.surface,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            );
          }).toList()),
          const SizedBox(height: 20),

          // Level
          Text('📊 Cấp độ', style: textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          Row(children: _levels.map((l) {
            final selected = _level == l.$1;
            return Expanded(child: Padding(
              padding: EdgeInsets.only(right: l != _levels.last ? 8 : 0),
              child: ChoiceChip(
                selected: selected, onSelected: (_) => setState(() => _level = l.$1),
                label: SizedBox(width: double.infinity, child: Text(l.$1, textAlign: TextAlign.center)),
                labelStyle: TextStyle(color: selected ? cs.onPrimary : cs.onSurface, fontWeight: FontWeight.bold),
                selectedColor: cs.primary, backgroundColor: cs.surface,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              ),
            ));
          }).toList()),
          const SizedBox(height: 20),

          // Warmup & Cooldown
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(color: cs.surface, borderRadius: BorderRadius.circular(16), border: Border.all(color: cs.outline.withValues(alpha: 0.1))),
            child: Column(children: [
              SwitchListTile(
                title: const Text('Khởi động (Warm-up)'),
                subtitle: Text(_includeWarmup ? 'Đã bật' : 'Đã tắt', style: TextStyle(color: _includeWarmup ? cs.primary : cs.onSurfaceVariant)),
                value: _includeWarmup, onChanged: (v) => setState(() => _includeWarmup = v),
                activeColor: cs.primary, contentPadding: EdgeInsets.zero,
              ),
              const Divider(height: 1),
              SwitchListTile(
                title: const Text('Giãn cơ cuối buổi (Cool-down)'),
                subtitle: Text(_includeCooldown ? 'Đã bật' : 'Đã tắt', style: TextStyle(color: _includeCooldown ? cs.primary : cs.onSurfaceVariant)),
                value: _includeCooldown, onChanged: (v) => setState(() => _includeCooldown = v),
                activeColor: cs.primary, contentPadding: EdgeInsets.zero,
              ),
            ]),
          ),
          const SizedBox(height: 24),

          // Generate
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: (_workoutType.isEmpty || _level.isEmpty || _isGenerating) ? null : _generate,
              icon: _isGenerating ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2)) : const Icon(Icons.auto_awesome),
              label: Text(_isGenerating ? 'AI đang thiết kế...' : '🚀 Tạo Buổi Tập Bằng AI'),
            ),
          ),
          const SizedBox(height: 24),

          // Result
          if (_result.isNotEmpty) ...[
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                gradient: LinearGradient(colors: [cs.primary.withValues(alpha: 0.08), cs.secondary.withValues(alpha: 0.05)]),
                borderRadius: BorderRadius.circular(16),
                border: Border.all(color: cs.primary.withValues(alpha: 0.2)),
              ),
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Row(children: [
                  Icon(Icons.auto_awesome, color: cs.primary),
                  const SizedBox(width: 8),
                  Text('Buổi tập AI đã sẵn sàng!', style: textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold, color: cs.primary)),
                ]),
                const SizedBox(height: 4),
                Text('$_totalDuration phút  •  $_workoutType  •  $_level  •  $_totalCalories kcal  •  ${_result.length} bài', style: textTheme.bodySmall?.copyWith(color: cs.onSurfaceVariant)),
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.all(10),
                  decoration: BoxDecoration(
                    color: cs.primary.withValues(alpha: 0.05),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.info_outline, color: Colors.blue, size: 18),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          'Lịch tập này đã được tự động lưu vào Lộ trình tập luyện của bạn. Hãy quay về màn hình chính và nhấn chọn ngày này để bắt đầu tập luyện nhé! 🥰',
                          style: TextStyle(fontSize: 11, color: cs.onSurfaceVariant, height: 1.3),
                        ),
                      ),
                    ],
                  ),
                ),
                const Divider(height: 24),
                ...List.generate(_result.length, (i) {
                  final e = _result[i];
                  final phase = e['phase'] as String? ?? 'Core';
                  final isSpecial = phase == 'Warm-up' || phase == 'Cool-down';
                  final name = e['name'] as String? ?? '';
                  final dur = e['durationMins'] ?? 0;
                  final cal = (e['caloriesBurned'] ?? 0).round();
                  final sets = e['sets'] ?? 0;
                  final reps = e['reps'] ?? 0;
                  final rest = e['restTimeSeconds'] ?? 0;
                  final muscle = e['muscle'] as String? ?? '';

                  return Container(
                    margin: const EdgeInsets.only(bottom: 8),
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                    decoration: BoxDecoration(
                      color: isSpecial ? cs.primary.withValues(alpha: 0.05) : null,
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Row(children: [
                      Container(
                        width: 28, height: 28, alignment: Alignment.center,
                        decoration: BoxDecoration(
                          color: isSpecial ? Colors.teal.withValues(alpha: 0.15) : cs.primary.withValues(alpha: 0.1),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: isSpecial
                            ? Icon(Icons.spa, size: 16, color: Colors.teal)
                            : Text('${i + 1}', style: TextStyle(fontWeight: FontWeight.bold, color: cs.primary, fontSize: 12)),
                      ),
                      const SizedBox(width: 12),
                      Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                        Text(name, style: TextStyle(fontWeight: isSpecial ? FontWeight.normal : FontWeight.w600)),
                        if (isSpecial)
                          Text(phase, style: TextStyle(fontSize: 11, color: cs.onSurfaceVariant))
                        else
                          Text(
                            '$sets hiệp × $reps lần  •  Nghỉ: ${rest}s${muscle.isNotEmpty ? '  •  $muscle' : ''}',
                            style: TextStyle(fontSize: 11, color: cs.onSurfaceVariant),
                          ),
                      ])),
                      Text('$dur ph', style: textTheme.bodySmall?.copyWith(color: cs.onSurfaceVariant)),
                      const SizedBox(width: 8),
                      Text('$cal kcal', style: textTheme.bodySmall?.copyWith(color: cs.primary, fontWeight: FontWeight.bold)),
                    ]),
                  );
                }),
              ]),
            ),
            const SizedBox(height: 20),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () {
                  Navigator.pop(context, true); // Hoàn tất và quay lại, báo cho roadmap biết để reload
                },
                icon: const Icon(Icons.save_rounded),
                label: const Text('Lưu lịch tập & Quay lại Lộ trình'),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                ),
              ),
            ),
          ],
          const SizedBox(height: 40),
        ]),
      ),
    ),
  );
}
}
