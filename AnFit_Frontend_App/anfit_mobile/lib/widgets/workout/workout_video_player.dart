import 'package:flutter/material.dart';
import 'package:youtube_player_flutter/youtube_player_flutter.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../core/theme/app_colors.dart';

/// ═══════════════════════════════════════════════════════════
/// WorkoutVideoPlayer — Trình phát Video hướng dẫn bài tập YouTube
/// Tinh gọn, ổn định, tự động xử lý Error 150 (hạn chế phát nhúng)
/// kèm nút mở trực tiếp trên YouTube app.
/// ═══════════════════════════════════════════════════════════
class WorkoutVideoPlayer extends StatefulWidget {
  final String videoUrl;
  final double height;

  const WorkoutVideoPlayer({
    super.key,
    required this.videoUrl,
    this.height = 220,
  });

  @override
  State<WorkoutVideoPlayer> createState() => _WorkoutVideoPlayerState();
}

class _WorkoutVideoPlayerState extends State<WorkoutVideoPlayer> {
  YoutubePlayerController? _controller;
  bool _isPlayerReady = false;
  bool _hasError = false;
  String? _videoId;

  @override
  void initState() {
    super.initState();
    _videoId = YoutubePlayer.convertUrlToId(widget.videoUrl);

    if (_videoId != null) {
      _controller = YoutubePlayerController(
        initialVideoId: _videoId!,
        flags: const YoutubePlayerFlags(
          autoPlay: false,
          mute: false,
          isLive: false,
          forceHD: true,
          enableCaption: false,
          useHybridComposition: true,
        ),
      )..addListener(_listener);
    }
  }

  void _listener() {
    if (!mounted || _controller == null) return;
    if (_controller!.value.isReady && !_isPlayerReady) {
      setState(() => _isPlayerReady = true);
    }
    if (_controller!.value.hasError && !_hasError) {
      setState(() => _hasError = true);
    }
  }

  Future<void> _openInYouTube() async {
    final url = _videoId != null
        ? 'https://www.youtube.com/watch?v=$_videoId'
        : widget.videoUrl;
    final uri = Uri.parse(url);
    try {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    } catch (_) {}
  }

  @override
  void deactivate() {
    _controller?.pause();
    super.deactivate();
  }

  @override
  void dispose() {
    _controller?.removeListener(_listener);
    _controller?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    if (_videoId == null) {
      return _buildErrorPlaceholder(
        isDark,
        'Không tìm thấy đường dẫn video cho động tác này.',
        showYouTubeButton: false,
      );
    }

    if (_hasError) {
      final code = _controller?.value.errorCode ?? 150;
      return _buildErrorPlaceholder(
        isDark,
        code == 150 || code == 101
            ? 'Chủ sở hữu video giới hạn phát trong ứng dụng khác.\nBạn có thể mở xem trực tiếp trên YouTube.'
            : 'Không thể phát video (Mã lỗi $code). Vui lòng mở xem trên YouTube.',
        showYouTubeButton: true,
      );
    }

    return Container(
      height: widget.height,
      width: double.infinity,
      decoration: BoxDecoration(
        color: Colors.black,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.15),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      clipBehavior: Clip.antiAlias,
      child: Stack(
        alignment: Alignment.center,
        children: [
          YoutubePlayer(
            controller: _controller!,
            showVideoProgressIndicator: true,
            progressIndicatorColor: Colors.red,
            progressColors: const ProgressBarColors(
              playedColor: Colors.red,
              handleColor: Colors.redAccent,
              bufferedColor: Colors.white24,
              backgroundColor: Colors.white10,
            ),
            onReady: () {
              if (mounted) setState(() => _isPlayerReady = true);
            },
          ),
          if (!_isPlayerReady)
            Container(
              color: isDark ? AppColors.darkCard : AppColors.lightCard,
              child: const Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    SizedBox(
                      width: 28,
                      height: 28,
                      child: CircularProgressIndicator(strokeWidth: 2.5),
                    ),
                    SizedBox(height: 12),
                    Text(
                      'Đang chuẩn bị video hướng dẫn...',
                      style: TextStyle(fontSize: 12, color: Colors.grey),
                    ),
                  ],
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildErrorPlaceholder(bool isDark, String message, {required bool showYouTubeButton}) {
    return Container(
      height: widget.height,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: isDark ? const Color(0xFF1E1E1E) : const Color(0xFFF8F9FA),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Colors.red.withValues(alpha: 0.25)),
      ),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.smart_display_rounded, color: Colors.red, size: 38),
            const SizedBox(height: 8),
            Text(
              message,
              textAlign: TextAlign.center,
              style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600, height: 1.4),
            ),
            if (showYouTubeButton) ...[
              const SizedBox(height: 12),
              ElevatedButton.icon(
                onPressed: _openInYouTube,
                icon: const Icon(Icons.open_in_new, size: 15),
                label: const Text('Mở xem trên YouTube', style: TextStyle(fontSize: 12)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.red,
                  foregroundColor: Colors.white,
                  elevation: 0,
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
