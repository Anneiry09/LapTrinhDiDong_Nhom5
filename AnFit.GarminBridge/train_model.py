import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.ensemble import RandomForestRegressor
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_squared_error, mean_absolute_error, r2_score
import joblib
import os

print("=== BẮT ĐẦU QUÁ TRÌNH HUẤN LUYỆN MÔ HÌNH EXERCISE SCORER (HYBRID AI) ===")

# Thiết lập thư mục lưu trữ
current_dir = os.path.dirname(os.path.abspath(__file__))
dataset_path = os.path.join(current_dir, "exercise_suitability_dataset.csv")
report_path = os.path.join(current_dir, "training_report.txt")
model_path = os.path.join(current_dir, "exercise_scorer.pkl")

# 1. Đọc Dataset từ file CSV đã thu thập/chuẩn bị sẵn
print(f"1. Đang tải tập dữ liệu huấn luyện từ: {dataset_path}...")
if not os.path.exists(dataset_path):
    raise FileNotFoundError(f"Không tìm thấy file dataset tại {dataset_path}. Vui lòng chuẩn bị file CSV dữ liệu huấn luyện trước.")

df = pd.read_csv(dataset_path, encoding='utf-8')
num_samples = len(df)
print(f"✅ Đã tải thành công bộ dữ liệu thực tế gồm {num_samples} mẫu.")

# 2. Tiền xử lý và Train/Test Split
print("2. Đang phân tách dữ liệu Train/Test (Tỉ lệ 80/20)...")
X = df.drop(columns=['Suitability_Score'])
y = df['Suitability_Score']

X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

# 3. Huấn luyện mô hình Random Forest
print("3. Bắt đầu huấn luyện Random Forest Regressor (100 Trees, max_depth=10)...")
model = RandomForestRegressor(n_estimators=100, max_depth=10, random_state=42, n_jobs=-1)
model.fit(X_train, y_train)

# 4. Đánh giá mô hình chi tiết
predictions = model.predict(X_test)
mse = mean_squared_error(y_test, predictions)
rmse = np.sqrt(mse)
mae = mean_absolute_error(y_test, predictions)
r2 = r2_score(y_test, predictions)
accuracy_percentage = (1 - (mae / 10.0)) * 100 # Độ chính xác tương đối trên thang điểm 10

print(f"   -> Sai số bình phương trung bình (MSE): {mse:.4f}")
print(f"   -> Căn sai số bình phương trung bình (RMSE): {rmse:.4f}")
print(f"   -> Sai số tuyệt đối trung bình (MAE): {mae:.4f}")
print(f"   -> Hệ số xác định (R-squared / R2 Score): {r2:.4f} ({r2*100:.2f}%)")
print(f"   -> Độ chính xác tương đối (Accuracy): {accuracy_percentage:.2f}%")

# Lưu báo cáo dạng Text
with open(report_path, "w", encoding="utf-8") as f:
    f.write("========================================================\n")
    f.write("         BÁO CÁO KẾT QUẢ HUẤN LUYỆN MÔ HÌNH AI ANFIT\n")
    f.write("========================================================\n\n")
    f.write(f"Mô hình sử dụng: Random Forest Regressor (100 n_estimators, 10 max_depth)\n")
    f.write(f"Kích thước bộ dữ liệu (Dataset size): {num_samples} mẫu\n")
    f.write(f"Phân chia tập dữ liệu: 80% Huấn luyện (Train), 20% Kiểm thử (Test)\n")
    f.write(f"Đường dẫn file dataset: exercise_suitability_dataset.csv\n\n")
    f.write("CHỈ SỐ ĐÁNH GIÁ ĐỘ CHÍNH XÁC:\n")
    f.write("--------------------------------------------------------\n")
    f.write(f"1. Mean Squared Error (MSE): {mse:.6f}\n")
    f.write(f"2. Root Mean Squared Error (RMSE): {rmse:.6f} (điểm lệch trên thang 10)\n")
    f.write(f"3. Mean Absolute Error (MAE): {mae:.6f} (sai số tuyệt đối trung bình)\n")
    f.write(f"4. R-squared (R2 Score): {r2:.6f} (Mô hình giải thích được {r2*100:.2f}% phương sai dữ liệu)\n")
    f.write(f"5. Độ chính xác tương đối (Accuracy): {accuracy_percentage:.2f}% (Tính trên thang điểm 10)\n\n")
    f.write("ĐẶC TRƯNG ĐẦU VÀO (FEATURES) VÀ Ý NGHĨA:\n")
    f.write("- User_Age: Tuổi người dùng (16 - 65)\n")
    f.write("- User_BMI: Chỉ số khối cơ thể (18.0 - 35.0)\n")
    f.write("- Is_Male: Giới tính (1: Nam, 0: Nữ)\n")
    f.write("- Target_Goal: Mục tiêu (1: Giảm cân, 2: Duy trì, 3: Tăng cơ)\n")
    f.write("- Fitness_Level: Thể lực (1: Mới tập, 2: Trung bình, 3: Nâng cao)\n")
    f.write("- Exercise_Type: Loại bài tập (1: Bodyweight, 2: Cardio, 3: Tạ đơn, 4: Máy tập, 5: Yoga)\n")
    f.write("- Exercise_BaseCalories: Calo đốt cháy cơ bản (10 - 150 kcal)\n")
    f.write("NHÃN ĐẦU RA (LABEL):\n")
    f.write("- Suitability_Score: Điểm phù hợp bài tập với người dùng (0.0 - 10.0)\n")
    f.write("========================================================\n")

print(f"✅ Đã ghi báo cáo kết quả huấn luyện tại: {report_path}")

# 5. Vẽ và lưu các biểu đồ (Visualizations)
print("5. Đang tạo các biểu đồ trực quan...")
sns.set_theme(style="whitegrid")

# Biểu đồ 1: Phân bố điểm phù hợp (Distribution of Suitability Scores)
plt.figure(figsize=(8, 5))
sns.histplot(df['Suitability_Score'], kde=True, color='teal', bins=30)
plt.title('Distribution of Exercise Suitability Scores (AnFit)', fontsize=14, fontweight='bold', pad=15)
plt.xlabel('Suitability Score (0 - 10)', fontsize=12)
plt.ylabel('Count / Frequency', fontsize=12)
plt.tight_layout()
dist_chart_path = os.path.join(current_dir, "dataset_distribution.png")
plt.savefig(dist_chart_path, dpi=300)
plt.close()
print(f"   -> Đã lưu biểu đồ phân bố tại: {dist_chart_path}")

# Biểu đồ 2: Tầm quan trọng của các đặc trưng (Feature Importance)
importances = model.feature_importances_
features_list = X.columns
feat_imp = pd.Series(importances, index=features_list).sort_values(ascending=True)

plt.figure(figsize=(9, 5))
# Custom colors
colors = sns.color_palette("viridis", len(feat_imp))
feat_imp.plot(kind='barh', color=colors)
plt.title('AI Model Feature Importance Analysis', fontsize=14, fontweight='bold', pad=15)
plt.xlabel('Importance Score', fontsize=12)
plt.ylabel('Features', fontsize=12)
plt.tight_layout()
feat_chart_path = os.path.join(current_dir, "feature_importance.png")
plt.savefig(feat_chart_path, dpi=300)
plt.close()
print(f"   -> Đã lưu biểu đồ tầm quan trọng đặc trưng tại: {feat_chart_path}")

# Biểu đồ 3: So sánh Giá trị thực tế vs Dự đoán (Actual vs Predicted Scores)
plt.figure(figsize=(8, 6))
# Lấy mẫu ngẫu nhiên 500 điểm từ test set để vẽ cho đẹp mắt, không bị quá dày đặc
sample_indices = np.random.choice(len(y_test), min(500, len(y_test)), replace=False)
y_test_sample = np.array(y_test)[sample_indices]
pred_sample = predictions[sample_indices]

sns.scatterplot(x=y_test_sample, y=pred_sample, alpha=0.6, color='royalblue', edgecolors='w')
# Đường phân giác hoàn hảo (y = x)
plt.plot([0, 10], [0, 10], color='crimson', linestyle='--', linewidth=2, label='Perfect Prediction (y = x)')
plt.title('Actual vs Predicted Suitability Scores', fontsize=14, fontweight='bold', pad=15)
plt.xlabel('Actual Score (Coach Label)', fontsize=12)
plt.ylabel('Predicted Score (AI Model)', fontsize=12)
plt.xlim(0, 10)
plt.ylim(0, 10)
plt.legend(loc='upper left')
plt.tight_layout()
pred_chart_path = os.path.join(current_dir, "actual_vs_predicted.png")
plt.savefig(pred_chart_path, dpi=300)
plt.close()
print(f"   -> Đã lưu biểu đồ thực tế vs dự đoán tại: {pred_chart_path}")

# 6. Lưu mô hình (.pkl)
joblib.dump(model, model_path)
print(f"6. ✅ Đã lưu mô hình cập nhật thành công tại: {model_path}")
print("=== HOÀN TẤT TRỌN VẸN ===")
