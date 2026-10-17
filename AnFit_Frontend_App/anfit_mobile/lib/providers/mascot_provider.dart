import 'package:flutter_riverpod/flutter_riverpod.dart';

/// Trạng thái linh vật được chọn
class MascotState {
  final String assetName; // e.g. 'Bo_Coach.json'
  final String name; // e.g. 'Bơ Coach 🥑'
  final String description; // Mô tả ngắn

  const MascotState({
    required this.assetName,
    required this.name,
    required this.description,
  });
}

const List<MascotState> availableMascots = [
  MascotState(
    assetName: 'Bo_Coach.json',
    name: 'Bơ Coach 🥑',
    description: 'Chú Bơ mũm mĩm, vui nhộn và vô cùng ấm áp.',
  ),
  MascotState(
    assetName: 'potato.json',
    name: 'Potato Coach 🥔',
    description: 'Bác Khoai Tây hài hước, năng lượng và tận tụy.',
  ),
  MascotState(
    assetName: 'nam_coach.json',
    name: 'Nam Coach 🏋️‍♂️',
    description: 'Huấn luyện viên Nam chuyên nghiệp, gắt gao & kỷ luật.',
  ),
];

class MascotNotifier extends StateNotifier<MascotState> {
  MascotNotifier() : super(availableMascots[0]); // Default to Bo Coach

  void selectMascot(String assetName) {
    final found = availableMascots.firstWhere(
      (m) => m.assetName == assetName,
      orElse: () => availableMascots[0],
    );
    state = found;
  }
}

final mascotProvider = StateNotifierProvider<MascotNotifier, MascotState>((ref) {
  return MascotNotifier();
});
