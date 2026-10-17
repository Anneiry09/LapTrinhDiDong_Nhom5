import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../../widgets/anfit_toast.dart';

/// 6.1 — Hồ sơ cá nhân (xem/sửa)
class ProfileScreen extends ConsumerStatefulWidget {
  const ProfileScreen({super.key});
  @override
  ConsumerState<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends ConsumerState<ProfileScreen> {
  Map<String, dynamic>? _profile;
  bool _loading = true;
  bool _saving = false;

  late TextEditingController _nameCtrl;
  late TextEditingController _heightCtrl;
  late TextEditingController _budgetCtrl;

  @override
  void initState() {
    super.initState();
    _nameCtrl = TextEditingController();
    _heightCtrl = TextEditingController();
    _budgetCtrl = TextEditingController();
    _load();
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    _heightCtrl.dispose();
    _budgetCtrl.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    try {
      final p = await ref.read(userServiceProvider).getProfile();
      if (mounted) {
        setState(() {
          _profile = p;
          _nameCtrl.text = p['fullName'] ?? '';
          _heightCtrl.text = '${p['heightCm'] ?? ''}';
          _budgetCtrl.text = '${(p['dailyBudget'] ?? 80000.0).toDouble().toStringAsFixed(0)}';
          _loading = false;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _save() async {
    setState(() => _saving = true);
    try {
      await ref.read(userServiceProvider).updateProfile({
        'fullName': _nameCtrl.text.trim(),
        'heightCm': double.tryParse(_heightCtrl.text) ?? 170,
        'dailyBudget': double.tryParse(_budgetCtrl.text) ?? 80000,
      });
      if (mounted) {
        AnFitToast.show(context, 'Đã cập nhật hồ sơ thành công!');
        Navigator.pop(context);
      }
    } on DioException catch (e) {
      if (mounted) {
        setState(() => _saving = false);
        AnFitToast.show(context, ApiClient.errorMessage(e), isError: true);
      }
    } catch (_) {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(title: const Text('Hồ Sơ Cá Nhân')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : ListView(padding: const EdgeInsets.all(20), children: [
              // Avatar
              Center(child: CircleAvatar(radius: 48, backgroundColor: cs.primary.withValues(alpha: 0.1), child: Icon(Icons.person, size: 48, color: cs.primary))),
              const SizedBox(height: 24),
              // Name
              TextField(controller: _nameCtrl, decoration: const InputDecoration(labelText: 'Họ và tên', prefixIcon: Icon(Icons.person_outline))),
              const SizedBox(height: 16),
              // Email (read-only)
              TextField(readOnly: true, decoration: InputDecoration(labelText: 'Email', prefixIcon: const Icon(Icons.email_outlined), hintText: _profile?['email'] ?? '')),
              const SizedBox(height: 16),
              // Height
              TextField(controller: _heightCtrl, keyboardType: TextInputType.number, decoration: const InputDecoration(labelText: 'Chiều cao (cm)', prefixIcon: Icon(Icons.height))),
              const SizedBox(height: 16),
              // Daily Budget
              TextField(controller: _budgetCtrl, keyboardType: TextInputType.number, decoration: const InputDecoration(labelText: 'Ngân sách ăn uống ngày (đ)', prefixIcon: Icon(Icons.payments_outlined))),
              const SizedBox(height: 16),
              // Gender (read-only)
              _infoTile('Giới tính', _profile?['gender'] == 1 ? 'Nam' : (_profile?['gender'] == 2 ? 'Nữ' : 'Chưa xác định'), Icons.wc),
              _infoTile('Ngày sinh', _formatDob(_profile?['dateOfBirth']), Icons.cake),
              _infoTile('Mục tiêu', _goalLabel(_profile?['targetGoal']), Icons.flag),
              const SizedBox(height: 32),
              ElevatedButton(onPressed: _saving ? null : _save, child: _saving ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2)) : const Text('Lưu thay đổi')),
            ]),
    );
  }

  Widget _infoTile(String label, String value, IconData icon) {
    final cs = Theme.of(context).colorScheme;
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(children: [
        Icon(icon, size: 20, color: cs.onSurfaceVariant),
        const SizedBox(width: 12),
        Text(label, style: TextStyle(color: cs.onSurfaceVariant)),
        const Spacer(),
        Text(value, style: const TextStyle(fontWeight: FontWeight.bold)),
      ]),
    );
  }

  String _formatDob(dynamic dob) {
    if (dob == null) return '—';
    try {
      final date = DateTime.parse(dob.toString());
      final day = date.day.toString().padLeft(2, '0');
      final month = date.month.toString().padLeft(2, '0');
      final year = date.year;
      // Tính tuổi
      final now = DateTime.now();
      int age = now.year - date.year;
      if (now.month < date.month || (now.month == date.month && now.day < date.day)) {
        age--;
      }
      return '$day/$month/$year ($age tuổi)';
    } catch (_) {
      return '—';
    }
  }

  String _goalLabel(int? goal) => switch (goal) { 1 => 'Giảm cân', 2 => 'Tăng cơ', _ => 'Duy trì' };
}
