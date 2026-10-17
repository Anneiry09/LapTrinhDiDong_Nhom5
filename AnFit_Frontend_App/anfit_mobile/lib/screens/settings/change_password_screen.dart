import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../../widgets/anfit_toast.dart';

/// 6.7 — Đổi mật khẩu
class ChangePasswordScreen extends ConsumerStatefulWidget {
  const ChangePasswordScreen({super.key});
  @override
  ConsumerState<ChangePasswordScreen> createState() => _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends ConsumerState<ChangePasswordScreen> {
  final _currentCtrl = TextEditingController();
  final _newCtrl = TextEditingController();
  final _confirmCtrl = TextEditingController();
  bool _saving = false;
  bool _obscureCurrent = true;
  bool _obscureNew = true;

  @override
  void dispose() {
    _currentCtrl.dispose();
    _newCtrl.dispose();
    _confirmCtrl.dispose();
    super.dispose();
  }

  Future<void> _changePassword() async {
    if (_newCtrl.text != _confirmCtrl.text) {
      AnFitToast.show(context, 'Mật khẩu mới không khớp!', isError: true);
      return;
    }
    if (_newCtrl.text.length < 6) {
      AnFitToast.show(context, 'Mật khẩu mới tối thiểu 6 ký tự', isError: true);
      return;
    }
    setState(() => _saving = true);
    try {
      await ref.read(userServiceProvider).changePassword(_currentCtrl.text, _newCtrl.text);
      if (mounted) {
        AnFitToast.show(context, 'Đổi mật khẩu thành công!');
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
    return Scaffold(
      appBar: AppBar(title: const Text('Đổi Mật Khẩu')),
      body: ListView(padding: const EdgeInsets.all(20), children: [
        TextField(
          controller: _currentCtrl,
          obscureText: _obscureCurrent,
          decoration: InputDecoration(
            labelText: 'Mật khẩu hiện tại',
            prefixIcon: const Icon(Icons.lock_outline),
            suffixIcon: IconButton(icon: Icon(_obscureCurrent ? Icons.visibility_off : Icons.visibility), onPressed: () => setState(() => _obscureCurrent = !_obscureCurrent)),
          ),
        ),
        const SizedBox(height: 16),
        TextField(
          controller: _newCtrl,
          obscureText: _obscureNew,
          decoration: InputDecoration(
            labelText: 'Mật khẩu mới',
            prefixIcon: const Icon(Icons.lock),
            suffixIcon: IconButton(icon: Icon(_obscureNew ? Icons.visibility_off : Icons.visibility), onPressed: () => setState(() => _obscureNew = !_obscureNew)),
          ),
        ),
        const SizedBox(height: 16),
        TextField(
          controller: _confirmCtrl,
          obscureText: _obscureNew,
          decoration: const InputDecoration(labelText: 'Xác nhận mật khẩu mới', prefixIcon: Icon(Icons.lock)),
        ),
        const SizedBox(height: 32),
        ElevatedButton(
          onPressed: _saving ? null : _changePassword,
          child: _saving ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2)) : const Text('Đổi mật khẩu'),
        ),
      ]),
    );
  }
}
