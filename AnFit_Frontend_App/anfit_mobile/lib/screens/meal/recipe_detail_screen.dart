import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../providers/service_providers.dart';
import '../../services/api_client.dart';
import '../../theme/anfit_theme.dart'; // Import theme mới của chúng ta

/// Màn hình chi tiết công thức nấu ăn - Thiết kế 2026 Premium Wellness
/// - Background: Deep Charcoal tĩnh lặng.
/// - Header: Ảnh món ăn mờ dần tinh tế vào nền không gian.
/// - Body: Sử dụng GlassmorphismCard cho macros, nguyên liệu và hướng dẫn AI.
/// - Action: Ghim AiGlowingButton lơ lửng ở dưới cùng (Spatial floating button).
class RecipeDetailScreen extends ConsumerStatefulWidget {
  final int recipeId;
  final String? recipeName;

  const RecipeDetailScreen({super.key, required this.recipeId, this.recipeName});

  @override
  ConsumerState<RecipeDetailScreen> createState() => _RecipeDetailScreenState();
}

class _RecipeDetailScreenState extends ConsumerState<RecipeDetailScreen> {
  bool _loadingDetail = true;
  bool _loadingInstructions = true;
  String? _error;
  Map<String, dynamic>? _recipe;
  List<String> _steps = [];
  String _source = '';
  final Set<int> _checkedSteps = {};

  double _toDouble(dynamic val) {
    if (val == null) return 0.0;
    if (val is num) return val.toDouble();
    if (val is String) return double.tryParse(val) ?? 0.0;
    return 0.0;
  }

  int _toInt(dynamic val) {
    if (val == null) return 0;
    if (val is num) return val.toInt();
    if (val is String) return int.tryParse(val) ?? 0;
    return 0;
  }


  @override
  void initState() {
    super.initState();
    _loadRecipeDetail();
  }

  Future<void> _loadRecipeDetail() async {
    try {
      final data = await ref.read(nutritionServiceProvider).getRecipeDetail(widget.recipeId);
      if (!mounted) return;
      setState(() {
        _recipe = data;
        _loadingDetail = false;
      });
      // Tự động kích hoạt gọi AI sinh hướng dẫn nấu ăn
      _loadInstructions(data);
    } on DioException catch (e) {
      if (mounted) {
        setState(() {
          _error = ApiClient.errorMessage(e);
          _loadingDetail = false;
        });
      }
    }
  }

  Future<void> _loadInstructions(Map<String, dynamic> recipe) async {
    setState(() {
      _loadingInstructions = true;
    });

    final List<Map<String, dynamic>> ingredients = (recipe['ingredients'] as List?)?.map((i) {
      if (i is Map) {
        return <String, dynamic>{
          'name': (i['name'] ?? '').toString(),
          'quantityGrams': _toDouble(i['quantityGrams']),
          'role': _toInt(i['role'] ?? 1),
        };
      }
      return <String, dynamic>{};
    }).where((e) => e.isNotEmpty).toList() ?? [];


    try {
      final result = await ref.read(nutritionServiceProvider).generateInstructions(
        recipeName: recipe['name'] ?? '',
        ingredients: ingredients,
      );
      if (!mounted) return;
      setState(() {
        _steps = (result['steps'] as List?)?.map((e) => e.toString()).toList() ?? [];
        _source = result['source'] ?? 'fallback';
        _loadingInstructions = false;
      });
    } on DioException catch (_) {
      if (mounted) {
        setState(() {
          _loadingInstructions = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final name = (_recipe?['name'] ?? widget.recipeName ?? 'Chi tiết món ăn').toString();

    return Scaffold(
      backgroundColor: AnFitTheme.scaffoldBackground,
      // Ghim nút AI trôi nổi lơ lửng ở dưới cùng theo chuẩn Spatial Design
      bottomNavigationBar: _loadingDetail || _error != null
          ? null
          : _buildFloatingBottomBar(),
      body: _error != null
          ? _buildErrorState()
          : _loadingDetail
              ? const Center(child: CircularProgressIndicator(color: AnFitTheme.primaryAccent))
              : _buildContent(name),
    );
  }

  /// Nút bấm AI phát sáng được ghim trôi nổi lơ lửng trên lớp kính mờ dưới cùng
  Widget _buildFloatingBottomBar() {
    return ClipRect(
      child: BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 10, sigmaY: 10),
        child: Container(
          padding: EdgeInsets.fromLTRB(20, 12, 20, 12 + MediaQuery.of(context).padding.bottom),
          decoration: BoxDecoration(
            color: AnFitTheme.scaffoldBackground.withValues(alpha: 0.75),
            border: Border(
              top: BorderSide(
                color: Colors.white.withValues(alpha: 0.08),
                width: 0.5,
              ),
            ),
          ),
          child: AiGlowingButton(
            text: _loadingInstructions ? "Đang sinh hướng dẫn..." : "🪄 Hướng dẫn nấu với AI",
            icon: Icons.auto_awesome,
            onPressed: () {
              if (_recipe != null && !_loadingInstructions) {
                _loadInstructions(_recipe!);
              }
            },
          ),
        ),
      ),
    );
  }

  /// Màn hình hiển thị khi xảy ra lỗi kết nối
  Widget _buildErrorState() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 64, color: AnFitTheme.auroraPurple),
            const SizedBox(height: 16),
            Text(
              _error ?? 'Đã xảy ra lỗi kết nối',
              textAlign: TextAlign.center,
              style: GoogleFonts.inter(color: AnFitTheme.textSecondary, fontSize: 15),
            ),
            const SizedBox(height: 24),
            ElevatedButton.icon(
              onPressed: () {
                setState(() {
                  _error = null;
                  _loadingDetail = true;
                });
                _loadRecipeDetail();
              },
              icon: const Icon(Icons.refresh),
              label: const Text('Thử lại'),
              style: ElevatedButton.styleFrom(
                backgroundColor: AnFitTheme.primaryAccent,
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 24),
              ),
            ),
          ],
        ),
      ),
    );
  }

  /// Nội dung chi tiết món ăn hoàn chỉnh
  Widget _buildContent(String recipeName) {
    final r = _recipe!;
    final cal = _toDouble(r['totalCalories']);
    final pro = _toDouble(r['totalProtein']);
    final carb = _toDouble(r['totalCarbs']);
    final fat = _toDouble(r['totalFat']);
    final cookTime = _toInt(r['cookingTimeMinutes']);
    final price = _toInt(r['estimatedPrice']);
    final List<Map<String, dynamic>> ingredients = (r['ingredients'] as List?)?.map((i) {
      if (i is Map) {
        return Map<String, dynamic>.from(i);
      }
      return <String, dynamic>{};
    }).toList() ?? [];
    
    // Placeholder ảnh món ăn đẹp mắt ngẫu nhiên nếu không có ImageUrl từ DB
    final rawImageUrl = r['imageUrl'] as String?;
    final imageUrl = (rawImageUrl == null || rawImageUrl.trim().isEmpty)
        ? "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?q=80&w=600&auto=format&fit=crop"
        : rawImageUrl.trim();

    return CustomScrollView(
      physics: const BouncingScrollPhysics(),
      slivers: [
        // 1. Header: Ảnh món ăn mờ dần vào nền (Spatial Image Header)
        SliverAppBar(
          expandedHeight: 320,
          backgroundColor: Colors.transparent,
          elevation: 0,
          pinned: true,
          leading: Padding(
            padding: const EdgeInsets.only(left: 16.0),
            child: Center(
              child: ClipOval(
                child: BackdropFilter(
                  filter: ImageFilter.blur(sigmaX: 8, sigmaY: 8),
                  child: Container(
                    width: 40,
                    height: 40,
                    color: Colors.black.withValues(alpha: 0.3),
                    child: IconButton(
                      icon: const Icon(Icons.arrow_back_ios_new, size: 16, color: Colors.white),
                      onPressed: () => Navigator.pop(context),
                    ),
                  ),
                ),
              ),
            ),
          ),
          flexibleSpace: FlexibleSpaceBar(
            background: Stack(
              fit: StackFit.expand,
              children: [
                // Ảnh món ăn
                imageUrl.startsWith('http')
                    ? CachedNetworkImage(
                        imageUrl: imageUrl,
                        fit: BoxFit.cover,
                        placeholder: (context, url) => Container(color: const Color(0xFF1E2630)),
                        errorWidget: (context, url, error) => Container(color: const Color(0xFF1E2630)),
                      )
                    : Image.asset(
                        'assets/images/recipes/$imageUrl',
                        fit: BoxFit.cover,
                        errorBuilder: (context, error, stackTrace) => Container(
                          color: const Color(0xFF1E2630),
                          child: const Center(
                            child: Icon(Icons.restaurant, color: Colors.white24, size: 48),
                          ),
                        ),
                      ),
                // Lớp phủ Gradient mờ dần vào nền Deep Charcoal tĩnh lặng
                const DecoratedBox(
                  decoration: BoxDecoration(
                    gradient: LinearGradient(
                      begin: Alignment.topCenter,
                      end: Alignment.bottomCenter,
                      colors: [
                        Colors.transparent,
                        Colors.transparent,
                        AnFitTheme.scaffoldBackground,
                      ],
                      stops: [0.0, 0.4, 1.0],
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),

        // 2. Nội dung món ăn cuộn trôi nổi
        SliverPadding(
          padding: const EdgeInsets.fromLTRB(20, 12, 20, 100),
          sliver: SliverList(
            delegate: SliverChildListDelegate([
              // Tên món ăn
              Text(
                recipeName,
                style: GoogleFonts.outfit(
                  fontSize: 26,
                  fontWeight: FontWeight.bold,
                  color: AnFitTheme.textPrimary,
                  letterSpacing: -0.3,
                ),
              ),
              const SizedBox(height: 16),

              // Thẻ Kính Mờ Macros & Thống kê
              GlassmorphismCard(
                padding: const EdgeInsets.all(18),
                child: Column(
                  children: [
                    // Grid chỉ số dinh dưỡng
                    Row(
                      children: [
                        _buildMacroItem("Kcal Consumed", cal.toStringAsFixed(0), "kcal", const Color(0xFFFF8A65)),
                        _buildDivider(),
                        _buildMacroItem("Protein", "${pro.toStringAsFixed(0)}g", "đạm", const Color(0xFF34D399)),
                        _buildDivider(),
                        _buildMacroItem("Carbs", "${carb.toStringAsFixed(0)}g", "tinh bột", const Color(0xFFFBBF24)),
                        _buildDivider(),
                        _buildMacroItem("Fat", "${fat.toStringAsFixed(0)}g", "béo", const Color(0xFFF87171)),
                      ],
                    ),
                    const SizedBox(height: 16),
                    // Đường chia kính nhạt
                    Container(height: 0.5, color: Colors.white.withValues(alpha: 0.08)),
                    const SizedBox(height: 16),
                    // Cook time + Price
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceAround,
                      children: [
                        if (cookTime > 0)
                          _buildDetailInfo(Icons.timer_outlined, '$cookTime phút', 'Thời gian chuẩn bị'),
                        if (price > 0)
                          _buildDetailInfo(Icons.payments_outlined, '${(price / 1000).toStringAsFixed(0)}K₫', 'Giá ước tính'),
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 28),

              // Tiêu đề phần nguyên liệu
              _buildSectionHeader(Icons.shopping_basket_outlined, "Nguyên liệu mua sắm"),
              const SizedBox(height: 12),

              // Thẻ Kính Mờ danh sách nguyên liệu
              GlassmorphismCard(
                padding: EdgeInsets.zero,
                child: Column(
                  children: List.generate(ingredients.length, (idx) {
                    final i = ingredients[idx];
                    final String name = (i['name'] ?? 'Nguyên liệu').toString();
                    final double grams = _toDouble(i['quantityGrams']);
                    final int role = _toInt(i['role'] ?? 1);

                    return Column(
                      children: [
                        Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                          child: Row(
                            children: [
                              // Chấm màu phân loại nguyên liệu
                              Container(
                                width: 8,
                                height: 8,
                                decoration: BoxDecoration(
                                  color: _getRoleColor(role),
                                  shape: BoxShape.circle,
                                ),
                              ),
                              const SizedBox(width: 14),
                              // Tên nguyên liệu
                              Expanded(
                                child: Text(
                                  name,
                                  style: GoogleFonts.inter(
                                    fontSize: 15,
                                    fontWeight: FontWeight.w500,
                                    color: AnFitTheme.textPrimary,
                                  ),
                                ),
                              ),
                              // Nhãn vai trò mờ nhẹ
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                decoration: BoxDecoration(
                                  color: _getRoleColor(role).withValues(alpha: 0.1),
                                  borderRadius: BorderRadius.circular(6),
                                ),
                                child: Text(
                                  _getRoleLabel(role),
                                  style: GoogleFonts.inter(
                                    fontSize: 10,
                                    fontWeight: FontWeight.bold,
                                    color: _getRoleColor(role),
                                  ),
                                ),
                              ),
                              const SizedBox(width: 16),
                              // Khối lượng
                              Text(
                                '${grams.toStringAsFixed(0)}g',
                                style: GoogleFonts.outfit(
                                  fontSize: 15,
                                  fontWeight: FontWeight.bold,
                                  color: AnFitTheme.primaryMint,
                                ),
                              ),
                            ],
                          ),
                        ),
                        if (idx < ingredients.length - 1)
                          Container(
                            margin: const EdgeInsets.only(left: 42),
                            height: 0.5,
                            color: Colors.white.withValues(alpha: 0.08),
                          ),
                      ],
                    );
                  }),
                ),
              ),
              const SizedBox(height: 28),

              // Tiêu đề phần hướng dẫn nấu
              _buildSectionHeader(Icons.menu_book_outlined, "Hướng dẫn nấu ăn chi tiết"),
              if (_source.isNotEmpty) ...[
                const SizedBox(height: 4),
                Row(
                  children: [
                    Icon(
                      _source == 'gemini' ? Icons.auto_awesome : Icons.menu_book_outlined,
                      size: 14,
                      color: _source == 'gemini' ? AnFitTheme.auroraCyan : AnFitTheme.textSecondary,
                    ),
                    const SizedBox(width: 6),
                    Text(
                      _source == 'gemini' ? 'Sinh bởi Gemini AI (Flash Model)' : 'Bộ hướng dẫn tự động',
                      style: GoogleFonts.inter(
                        fontSize: 12,
                        color: _source == 'gemini' ? AnFitTheme.auroraCyan : AnFitTheme.textSecondary,
                        fontStyle: FontStyle.italic,
                      ),
                    ),
                  ],
                ),
              ],
              const SizedBox(height: 12),

              // Danh sách các bước hướng dẫn
              _loadingInstructions
                  ? _buildShimmerLoading()
                  : _steps.isEmpty
                      ? _buildEmptyState()
                      : _buildInstructionsList(),
            ]),
          ),
        ),
      ],
    );
  }

  /// Tiêu đề phân đoạn mang phong cách sang trọng
  Widget _buildSectionHeader(IconData icon, String title) {
    return Row(
      children: [
        Icon(icon, size: 20, color: AnFitTheme.primaryAccent),
        const SizedBox(width: 8),
        Text(
          title,
          style: GoogleFonts.outfit(
            fontSize: 18,
            fontWeight: FontWeight.bold,
            color: AnFitTheme.textPrimary,
          ),
        ),
      ],
    );
  }

  /// Widget hiển thị chỉ số dinh dưỡng đơn lẻ
  Widget _buildMacroItem(String label, String value, String unit, Color color) {
    return Expanded(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            value,
            style: GoogleFonts.outfit(
              fontSize: 20,
              fontWeight: FontWeight.bold,
              color: color,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            unit,
            style: GoogleFonts.inter(
              fontSize: 11,
              fontWeight: FontWeight.w500,
              color: AnFitTheme.textSecondary,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDivider() {
    return Container(
      width: 0.5,
      height: 24,
      color: Colors.white.withValues(alpha: 0.08),
    );
  }

  /// Thông tin chi tiết chuẩn bị & giá cả
  Widget _buildDetailInfo(IconData icon, String text, String label) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: 0.04),
            shape: BoxShape.circle,
            border: Border.all(color: Colors.white.withValues(alpha: 0.08), width: 0.5),
          ),
          child: Icon(icon, size: 16, color: AnFitTheme.primaryMint),
        ),
        const SizedBox(width: 10),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              text,
              style: GoogleFonts.outfit(
                fontSize: 14,
                fontWeight: FontWeight.bold,
                color: AnFitTheme.textPrimary,
              ),
            ),
            Text(
              label,
              style: GoogleFonts.inter(
                fontSize: 10,
                color: AnFitTheme.textSecondary,
              ),
            ),
          ],
        ),
      ],
    );
  }

  /// Danh sách hướng dẫn nấu ăn tích hợp checklist kính mờ
  Widget _buildInstructionsList() {
    return Column(
      children: List.generate(_steps.length, (idx) {
        final step = _steps[idx];
        final checked = _checkedSteps.contains(idx);

        return Container(
          margin: const EdgeInsets.only(bottom: 10),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(18),
            // Trạng thái đã hoàn tất sẽ sáng nhẹ màu Emerald, chưa hoàn tất sẽ có lớp kính trong suốt
            color: checked
                ? AnFitTheme.primaryAccent.withValues(alpha: 0.08)
                : Colors.white.withValues(alpha: 0.03),
            border: Border.all(
              color: checked
                  ? AnFitTheme.primaryAccent.withValues(alpha: 0.3)
                  : Colors.white.withValues(alpha: 0.06),
              width: 0.5,
            ),
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(18),
            child: InkWell(
              borderRadius: BorderRadius.circular(18),
              onTap: () {
                setState(() {
                  if (checked) {
                    _checkedSteps.remove(idx);
                  } else {
                    _checkedSteps.add(idx);
                  }
                });
              },
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Checkbox hình tròn Wellness mượt mà
                    Container(
                      margin: const EdgeInsets.only(top: 2),
                      width: 22,
                      height: 22,
                      decoration: BoxDecoration(
                        color: checked ? AnFitTheme.primaryAccent : Colors.transparent,
                        shape: BoxShape.circle,
                        border: Border.all(
                          color: checked ? AnFitTheme.primaryAccent : Colors.white.withValues(alpha: 0.25),
                          width: 1.5,
                        ),
                      ),
                      child: checked
                          ? const Icon(Icons.check, size: 14, color: Colors.white)
                          : null,
                    ),
                    const SizedBox(width: 14),
                    // Nội dung bước nấu
                    Expanded(
                      child: Text(
                        step,
                        style: GoogleFonts.inter(
                          fontSize: 14,
                          height: 1.5,
                          color: checked ? AnFitTheme.textSecondary : AnFitTheme.textPrimary,
                          decoration: checked ? TextDecoration.lineThrough : null,
                          decorationColor: AnFitTheme.textSecondary,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        );
      }),
    );
  }

  /// Shimmer loading bằng kính mờ cực chất cho Spatial design
  Widget _buildShimmerLoading() {
    return Column(
      children: List.generate(4, (i) => Padding(
        padding: const EdgeInsets.only(bottom: 10),
        child: ClipRRect(
          borderRadius: BorderRadius.circular(16),
          child: _ShimmerBlock(
            height: 52,
            borderRadius: 16,
            baseColor: Colors.white.withValues(alpha: 0.04),
            highlightColor: Colors.white.withValues(alpha: 0.08),
          ),
        ),
      )),
    );
  }

  Widget _buildEmptyState() {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 32),
      child: Center(
        child: Column(
          children: [
            Icon(Icons.info_outline, size: 36, color: AnFitTheme.textSecondary.withValues(alpha: 0.5)),
            const SizedBox(height: 8),
            Text(
              'Chưa có dữ liệu hướng dẫn nấu ăn.',
              style: GoogleFonts.inter(color: AnFitTheme.textSecondary, fontSize: 13),
            ),
          ],
        ),
      ),
    );
  }

  // ── Helper Methods ──
  Color _getRoleColor(int role) {
    switch (role) {
      case 1:
        return AnFitTheme.primaryAccent; // Xanh ngọc lục bảo cho nguyên liệu Chính
      case 2:
        return const Color(0xFFFBBF24); // Vàng mật ong cho nguyên liệu Phụ
      case 3:
      default:
        return AnFitTheme.textSecondary; // Xám cho Gia vị
    }
  }

  String _getRoleLabel(int role) {
    switch (role) {
      case 1:
        return 'Chính';
      case 2:
        return 'Phụ';
      case 3:
      default:
        return 'Gia vị';
    }
  }
}

/// Animated Shimmer Block - Không gian mờ kính
class _ShimmerBlock extends StatefulWidget {
  final double height;
  final double borderRadius;
  final Color baseColor;
  final Color highlightColor;

  const _ShimmerBlock({
    required this.height,
    required this.borderRadius,
    required this.baseColor,
    required this.highlightColor,
  });

  @override
  State<_ShimmerBlock> createState() => _ShimmerBlockState();
}

class _ShimmerBlockState extends State<_ShimmerBlock> with SingleTickerProviderStateMixin {
  late AnimationController _controller;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1500),
    )..repeat();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _controller,
      builder: (context, child) {
        return Container(
          height: widget.height,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(widget.borderRadius),
            gradient: LinearGradient(
              begin: Alignment(-1.0 + 2.0 * _controller.value, 0),
              end: Alignment(-1.0 + 2.0 * _controller.value + 1.0, 0),
              colors: [
                widget.baseColor,
                widget.highlightColor,
                widget.baseColor,
              ],
            ),
          ),
        );
      },
    );
  }
}
