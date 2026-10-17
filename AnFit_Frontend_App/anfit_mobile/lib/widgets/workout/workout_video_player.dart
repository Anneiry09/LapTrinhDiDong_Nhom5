import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:youtube_player_flutter/youtube_player_flutter.dart';
import '../../core/theme/app_colors.dart'; // Fixed import path to core/theme/app_colors.dart

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
  late YoutubePlayerController _controller;
  bool _isPlayerReady = false;
  String? _videoId;

  @override
  void initState() {
    super.initState();
    // Extract YouTube Video ID from any YouTube URL format
    _videoId = YoutubePlayer.convertUrlToId(widget.videoUrl);

    if (_videoId != null) {
      _controller = YoutubePlayerController(
        initialVideoId: _videoId!,
        flags: const YoutubePlayerFlags(
          autoPlay: false, // Prevents autoplaying to save user data
          mute: false,
          isLive: false,
          forceHD: true, // Forces HD resolution
          enableCaption: false,
        ),
      )..addListener(_listener);
    }
  }

  void _listener() {
    if (mounted && _controller.value.isReady) {
      setState(() {
        _isPlayerReady = true;
      });
    }
  }

  @override
  void deactivate() {
    // Pauses video when widget is not active
    if (_videoId != null) {
      _controller.pause();
    }
    super.deactivate();
  }

  @override
  void dispose() {
    if (_videoId != null) {
      _controller.removeListener(_listener);
      _controller.dispose();
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    if (_videoId == null) {
      return _buildErrorPlaceholder(
        isDark,
        'Không tìm thấy video hướng dẫn cho động tác này.',
      );
    }

    return Container(
      height: widget.height,
      width: double.infinity,
      decoration: BoxDecoration(
        color: Colors.black,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.15),
            blurRadius: 16,
            offset: const Offset(0, 6),
          )
        ],
      ),
      clipBehavior: Clip.antiAlias,
      child: Stack(
        alignment: Alignment.center,
        children: [
          // Official YouTube Player widget
          YoutubePlayer(
            controller: _controller,
            showVideoProgressIndicator: true,
            progressIndicatorColor: AppColors.auroraCyan,
            progressColors: ProgressBarColors(
              playedColor: AppColors.auroraCyan,
              handleColor: AppColors.auroraCyan,
              bufferedColor: Colors.white24,
              backgroundColor: Colors.white10,
            ),
            onReady: () {
              setState(() {
                _isPlayerReady = true;
              });
            },
          ),

          // Premium Shimmer Overlay during player initialization
          if (!_isPlayerReady)
            Container(
              color: isDark ? AppColors.darkCard : AppColors.lightCard,
              child: Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    SizedBox(
                      width: 32,
                      height: 32,
                      child: CircularProgressIndicator(
                        strokeWidth: 3,
                        color: AppColors.auroraCyan,
                      ),
                    ),
                    const SizedBox(height: 14),
                    Text(
                      'Đang chuẩn bị video hướng dẫn...',
                      style: GoogleFonts.inter(
                        fontSize: 12,
                        color: Colors.grey,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildErrorPlaceholder(bool isDark, String message) {
    return Container(
      height: widget.height,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: isDark ? AppColors.darkCard : AppColors.lightCard,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.video_library_rounded,
              color: Colors.grey,
              size: 44,
            ),
            const SizedBox(height: 12),
            Text(
              message,
              textAlign: TextAlign.center,
              style: GoogleFonts.inter(
                fontSize: 13,
                color: Colors.grey,
                height: 1.4,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
