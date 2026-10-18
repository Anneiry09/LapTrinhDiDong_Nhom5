import 'package:flutter/material.dart';
import 'meal_roadmap_screen.dart';

/// MealPlanScreen — Forwarder giữ tương thích điều hướng
class MealPlanScreen extends StatelessWidget {
  final String? date;
  const MealPlanScreen({super.key, this.date});

  @override
  Widget build(BuildContext context) {
    return MealRoadmapScreen(initialDate: date);
  }
}
