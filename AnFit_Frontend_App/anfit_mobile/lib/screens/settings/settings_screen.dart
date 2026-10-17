import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import 'package:fl_chart/fl_chart.dart';
import '../../core/theme/theme_provider.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../../screens/auth/entry_screen.dart';
import 'profile_screen.dart';
import 'change_password_screen.dart';
import '../../widgets/anfit_toast.dart';

/// Màn hình Cài đặt & Quản lý tài khoản
class SettingsScreen extends ConsumerStatefulWidget {
  const SettingsScreen({super.key});
  @override
  ConsumerState<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends ConsumerState<SettingsScreen> {
  Map<String, dynamic>? _profile;

  @override
  void initState() {
    super.initState();
    _loadProfile();
  }

  Future<void> _loadProfile() async {
    try {
      final p = await ref.read(userServiceProvider).getProfile();
      if (mounted) setState(() => _profile = p);
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final isDark = ref.watch(themeModeProvider) == ThemeMode.dark;
    final name = _profile?['fullName'] ?? 'AnFit User';
    final email = _profile?['email'] ?? '';

    return Scaffold(
      appBar: AppBar(
        title: const Text('Cài Đặt'),
        actions: [
          IconButton(
            tooltip: 'Đăng xuất',
            icon: Icon(Icons.logout_rounded, color: cs.error),
            onPressed: () => _logout(context),
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 120),
        children: [
          // Profile Card
          InkWell(
            onTap: () async {
              await Navigator.push(context, MaterialPageRoute(builder: (_) => const ProfileScreen()));
              _loadProfile(); // Refresh after edit
            },
            borderRadius: BorderRadius.circular(16),
            child: Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(color: cs.surface, borderRadius: BorderRadius.circular(16), border: Border.all(color: cs.outline.withValues(alpha: 0.1))),
              child: Row(children: [
                CircleAvatar(radius: 30, backgroundColor: cs.primary.withValues(alpha: 0.1), child: Icon(Icons.person, size: 32, color: cs.primary)),
                const SizedBox(width: 16),
                Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(name as String, style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
                  Text(email as String, style: TextStyle(color: cs.onSurfaceVariant)),
                ])),
                Icon(Icons.edit, color: cs.onSurfaceVariant),
              ]),
            ),
          ),
          const SizedBox(height: 16),

          // Theme Toggle
          _settingsTile(context, icon: isDark ? Icons.dark_mode : Icons.light_mode, title: 'Giao diện ${isDark ? "Tối" : "Sáng"}',
            trailing: Switch(value: isDark, onChanged: (_) => ref.read(themeModeProvider.notifier).toggleTheme(), activeColor: cs.primary)),

          // Bù calo tập luyện
          _settingsTile(context, icon: Icons.local_fire_department_outlined, title: 'Cộng calo tập luyện vào mục tiêu',
            trailing: Switch(
              value: _profile?['isCompensateExercise'] ?? true,
              onChanged: (v) async {
                setState(() => _profile?['isCompensateExercise'] = v);
                try {
                  await ref.read(userServiceProvider).updateProfile({
                    'fullName': _profile?['fullName'] ?? '',
                    'gender': _profile?['gender'] ?? 0,
                    'heightCm': _profile?['heightCm'] ?? 0,
                    'dateOfBirth': _profile?['dateOfBirth'],
                    'targetGoal': _profile?['targetGoal'] ?? 0,
                    'dailyBudget': _profile?['dailyBudget'] ?? 0,
                    'isCompensateExercise': v,
                  });
                } catch (_) {
                  if (mounted) setState(() => _profile?['isCompensateExercise'] = !v);
                }
              },
              activeColor: cs.primary,
            ),
          ),

          const Divider(height: 32),

          _settingsTile(context, icon: Icons.person_outline, title: 'Hồ sơ cá nhân', onTap: () async {
            await Navigator.push(context, MaterialPageRoute(builder: (_) => const ProfileScreen()));
            _loadProfile();
          }),
          _settingsTile(context, icon: Icons.monitor_weight_outlined, title: 'Lịch sử cân nặng', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const BodyLogScreen()))),
          _settingsTile(context, icon: Icons.lock_outline, title: 'Đổi mật khẩu', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ChangePasswordScreen()))),

          const Divider(height: 32),

          // ── NÚT ĐĂNG XUẤT NỔI BẬT ──
          Container(
            width: double.infinity,
            margin: const EdgeInsets.symmetric(vertical: 4),
            child: ElevatedButton.icon(
              onPressed: () => _logout(context),
              icon: const Icon(Icons.logout_rounded, color: Colors.white, size: 20),
              label: const Text(
                'Đăng xuất tài khoản',
                style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 16),
              ),
              style: ElevatedButton.styleFrom(
                backgroundColor: cs.error,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                elevation: 0,
              ),
            ),
          ),
          const SizedBox(height: 12),

          // Xóa tài khoản (mờ hơn)
          Center(
            child: TextButton(
              onPressed: () => _showDeleteAccount(context),
              child: Text(
                'Xóa tài khoản',
                style: TextStyle(color: cs.error.withValues(alpha: 0.7), fontSize: 13),
              ),
            ),
          ),

          const SizedBox(height: 20),
          Center(child: Text('AnFit Mobile  •  Đồ án Môn học', style: TextStyle(fontSize: 12, color: cs.onSurfaceVariant))),
        ],
      ),
    );
  }

  Widget _settingsTile(BuildContext context, {required IconData icon, required String title, Widget? trailing, VoidCallback? onTap, Color? titleColor}) {
    final cs = Theme.of(context).colorScheme;
    return ListTile(
      leading: Icon(icon, color: titleColor ?? cs.onSurfaceVariant),
      title: Text(title, style: TextStyle(color: titleColor)),
      trailing: trailing ?? (onTap != null ? Icon(Icons.chevron_right, color: cs.onSurfaceVariant) : null),
      onTap: onTap,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
    );
  }

  Future<void> _logout(BuildContext context) async {
    final cs = Theme.of(context).colorScheme;
    final confirm = await showDialog<bool>(context: context, builder: (ctx) => AlertDialog(
      title: const Text('Đăng xuất'),
      content: const Text('Bạn có chắc muốn đăng xuất?'),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Hủy')),
        TextButton(onPressed: () => Navigator.pop(ctx, true), child: Text('Đăng xuất', style: TextStyle(color: cs.error))),
      ],
    ));
    if (confirm == true && context.mounted) {
      await ref.read(authServiceProvider).logout();
      if (context.mounted) {
        Navigator.pushAndRemoveUntil(context, MaterialPageRoute(builder: (_) => const EntryScreen()), (_) => false);
      }
    }
  }

  // 6.8 Xóa tài khoản
  Future<void> _showDeleteAccount(BuildContext context) async {
    final cs = Theme.of(context).colorScheme;
    final passCtrl = TextEditingController();
    final confirm = await showDialog<bool>(context: context, builder: (ctx) => AlertDialog(
      title: Text('Xóa tài khoản', style: TextStyle(color: cs.error)),
      content: Column(mainAxisSize: MainAxisSize.min, children: [
        const Text('Hành động này không thể hoàn tác. Nhập mật khẩu để xác nhận:'),
        const SizedBox(height: 12),
        TextField(controller: passCtrl, obscureText: true, decoration: const InputDecoration(labelText: 'Mật khẩu', prefixIcon: Icon(Icons.lock))),
      ]),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Hủy')),
        TextButton(onPressed: () => Navigator.pop(ctx, true), child: Text('Xóa vĩnh viễn', style: TextStyle(color: cs.error))),
      ],
    ));
    if (confirm == true && passCtrl.text.isNotEmpty && context.mounted) {
      try {
        await ref.read(userServiceProvider).deleteAccount(passCtrl.text);
        if (context.mounted) {
          Navigator.pushAndRemoveUntil(context, MaterialPageRoute(builder: (_) => const EntryScreen()), (_) => false);
        }
      } on DioException catch (e) {
        if (context.mounted) AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
      }
    }
    passCtrl.dispose();
  }
}

// ═══════════════════════════════════════
// 6.3 Body Log — Lịch sử cân nặng + fl_chart
// ═══════════════════════════════════════
class BodyLogScreen extends ConsumerStatefulWidget {
  const BodyLogScreen({super.key});
  @override
  ConsumerState<BodyLogScreen> createState() => _BodyLogScreenState();
}

class _BodyLogScreenState extends ConsumerState<BodyLogScreen> {
  List<Map<String, dynamic>> _logs = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final res = await ref.read(userServiceProvider).getBodyLogs(pageSize: 30);
      final data = res['data'] as List? ?? [];
      if (mounted) setState(() { _logs = data.cast<Map<String, dynamic>>(); _loading = false; });
    } catch (_) {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _addLog() async {
    final weightCtrl = TextEditingController();
    final result = await showDialog<bool>(context: context, builder: (ctx) => AlertDialog(
      title: const Text('Ghi nhận cân nặng'),
      content: TextField(controller: weightCtrl, keyboardType: TextInputType.number, decoration: const InputDecoration(labelText: 'Cân nặng (kg)', suffixText: 'kg')),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Hủy')),
        TextButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('Lưu')),
      ],
    ));
    if (result == true && weightCtrl.text.isNotEmpty) {
      try {
        await ref.read(userServiceProvider).addBodyLog(double.parse(weightCtrl.text));
        _load();
        if (mounted) AnFitToast.show(context, 'Đã ghi nhận thành công!');
      } on DioException catch (e) {
        if (mounted) AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
      }
    }
    weightCtrl.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(title: const Text('Lịch Sử Cân Nặng')),
      floatingActionButton: FloatingActionButton(onPressed: _addLog, child: const Icon(Icons.add)),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(onRefresh: _load, child: ListView(padding: const EdgeInsets.all(16), children: [
              // Stats summary
              if (_logs.isNotEmpty) ...[
                Container(
                  padding: const EdgeInsets.all(20),
                  decoration: BoxDecoration(gradient: LinearGradient(colors: [cs.primary.withValues(alpha: 0.1), cs.secondary.withValues(alpha: 0.05)]), borderRadius: BorderRadius.circular(16)),
                  child: Row(mainAxisAlignment: MainAxisAlignment.spaceAround, children: [
                    _stat('${(_logs.first['weight'] ?? 0).toStringAsFixed(1)}', 'kg hiện tại', cs.primary),
                    _stat('${(_logs.first['bmi'] ?? 0).toStringAsFixed(1)}', 'BMI', cs.secondary),
                    if (_logs.length > 1) _stat('${((_logs.first['weight'] ?? 0) - (_logs.last['weight'] ?? 0)).toStringAsFixed(1)}', 'kg thay đổi', Colors.green),
                  ]),
                ),
                const SizedBox(height: 20),
              ],
              // fl_chart
              if (_logs.length >= 2) ...[
                SizedBox(
                  height: 180,
                  child: LineChart(LineChartData(
                    gridData: const FlGridData(show: false),
                    titlesData: const FlTitlesData(show: false),
                    borderData: FlBorderData(show: false),
                    lineBarsData: [
                      LineChartBarData(
                        spots: _logs.reversed.toList().asMap().entries.map((e) => FlSpot(e.key.toDouble(), (e.value['weight'] as num?)?.toDouble() ?? 0)).toList(),
                        isCurved: true,
                        color: cs.primary,
                        barWidth: 3,
                        dotData: const FlDotData(show: true),
                        belowBarData: BarAreaData(show: true, color: cs.primary.withValues(alpha: 0.1)),
                      ),
                    ],
                  )),
                ),
                const SizedBox(height: 20),
              ],
              // Log list
              ..._logs.map((l) => Container(
                margin: const EdgeInsets.only(bottom: 8),
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(color: cs.surface, borderRadius: BorderRadius.circular(12), border: Border.all(color: cs.outline.withValues(alpha: 0.1))),
                child: Row(children: [
                  Text((l['recordedAt'] ?? '').toString().substring(0, 10), style: TextStyle(color: cs.onSurfaceVariant)),
                  const Spacer(),
                  Text('${(l['weight'] ?? 0).toStringAsFixed(1)} kg', style: const TextStyle(fontWeight: FontWeight.bold)),
                  const SizedBox(width: 16),
                  Text('BMI ${(l['bmi'] ?? 0).toStringAsFixed(1)}', style: TextStyle(color: cs.onSurfaceVariant)),
                ]),
              )),
              if (_logs.isEmpty) Center(child: Padding(padding: const EdgeInsets.all(40), child: Text('Chưa có dữ liệu cân nặng', style: TextStyle(color: cs.onSurfaceVariant)))),
            ])),
    );
  }

  Widget _stat(String value, String label, Color color) {
    return Column(children: [
      Text(value, style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: color)),
      Text(label, style: const TextStyle(fontSize: 12)),
    ]);
  }
}

