import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'core/theme/app_theme.dart';
import 'core/theme/theme_provider.dart';
import 'providers/service_providers.dart';
import 'screens/auth/entry_screen.dart';
import 'screens/home/dashboard_screen.dart';
import 'theme/anfit_theme.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initializeDateFormatting('vi', null);
  runApp(const ProviderScope(child: AnFitMobileApp()));
}

class AnFitMobileApp extends ConsumerWidget {
  const AnFitMobileApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final themeMode = ref.watch(themeModeProvider);

    return MaterialApp(
      title: 'AnFit Mobile',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.lightTheme,
      darkTheme: AnFitTheme.darkTheme,
      themeMode: themeMode,
      // Localization cho DatePicker và các widget Material hiển thị tiếng Việt
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: const [
        Locale('vi', 'VN'),
        Locale('en', 'US'),
      ],
      locale: const Locale('vi', 'VN'),
      home: const _SplashGate(),
    );
  }
}

/// Check token on startup → auto-login hoặc về EntryScreen
class _SplashGate extends ConsumerStatefulWidget {
  const _SplashGate();
  @override
  ConsumerState<_SplashGate> createState() => _SplashGateState();
}

class _SplashGateState extends ConsumerState<_SplashGate> {
  @override
  void initState() {
    super.initState();
    _checkAuth();
  }

  Future<void> _checkAuth() async {
    final hasToken = await ref.read(authServiceProvider).isLoggedIn();

    if (!mounted) return;
    final target = hasToken ? const DashboardScreen() : const EntryScreen();

    Navigator.pushReplacement(
      context,
      PageRouteBuilder(
        transitionDuration: const Duration(milliseconds: 400),
        pageBuilder: (_, __, ___) => target,
        transitionsBuilder: (_, anim, __, child) => FadeTransition(opacity: anim, child: child),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    // Splash screen tạm — hiện logo/mascot trong lúc check token
    return Scaffold(
      body: Center(
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Icon(Icons.fitness_center, size: 56, color: Theme.of(context).colorScheme.primary),
          const SizedBox(height: 12),
          Text('AnFit', style: Theme.of(context).textTheme.headlineMedium?.copyWith(fontWeight: FontWeight.w800)),
        ]),
      ),
    );
  }
}
