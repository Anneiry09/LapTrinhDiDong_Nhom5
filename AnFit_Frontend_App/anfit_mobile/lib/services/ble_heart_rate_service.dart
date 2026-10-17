import 'dart:async';
import 'package:flutter_blue_plus/flutter_blue_plus.dart';

/// ═══════════════════════════════════════════════════════════
/// BleHeartRateService — Dịch vụ Bluetooth Kết nối Nhịp tim thời gian thực
/// ═══════════════════════════════════════════════════════════
class BleHeartRateService {
  BluetoothDevice? _connectedDevice;
  StreamSubscription<List<int>>? _hrValueSubscription;
  StreamSubscription<BluetoothConnectionState>? _connStateSubscription;

  // StreamController phát nhịp tim thời gian thực
  final StreamController<int> _hrController = StreamController<int>.broadcast();
  Stream<int> get heartRateStream => _hrController.stream;

  // StreamController phát trạng thái kết nối
  final StreamController<BluetoothConnectionState> _connStateController =
      StreamController<BluetoothConnectionState>.broadcast();
  Stream<BluetoothConnectionState> get connectionStateStream => _connStateController.stream;

  BluetoothDevice? get connectedDevice => _connectedDevice;

  /// Bắt đầu quét tìm các thiết bị phát nhịp tim (Heart Rate service: 0x180D)
  Future<void> startScan() async {
    try {
      // Đảm bảo Bluetooth đã bật
      final adapterState = await FlutterBluePlus.adapterState.first;
      if (adapterState != BluetoothAdapterState.on) {
        throw Exception('Vui lòng bật Bluetooth trên điện thoại để tiếp tục.');
      }

      // Chỉ quét tìm các thiết bị chứa dịch vụ nhịp tim (0x180d) để tránh rác danh sách
      await FlutterBluePlus.startScan(
        timeout: const Duration(seconds: 15),
        withServices: [Guid("180d")],
      );
    } catch (e) {
      print('Lỗi khi quét BLE: $e');
      rethrow;
    }
  }

  /// Dừng quét thiết bị
  Future<void> stopScan() async {
    try {
      await FlutterBluePlus.stopScan();
    } catch (_) {}
  }

  /// Kết nối trực tiếp vào đồng hồ/thiết bị cảm biến nhịp tim
  Future<void> connectToDevice(BluetoothDevice device, {Function(String)? onProgress}) async {
    try {
      onProgress?.call('Đang chuẩn bị và dừng quét thiết bị...');
      await stopScan();
      
      onProgress?.call('Đang ngắt kết nối thiết bị cũ nếu có...');
      await disconnectDevice();

      _connectedDevice = device;
      
      onProgress?.call('Đang đăng ký kênh trạng thái...');
      // Đăng ký lắng nghe thay đổi trạng thái kết nối
      _connStateSubscription = device.connectionState.listen((state) {
        _connStateController.add(state);
        if (state == BluetoothConnectionState.disconnected) {
          _cleanUpConnection();
        }
      });

      onProgress?.call('Đang thực hiện kết nối BLE vật lý (tối đa 10s)...');
      // Thực hiện kết nối với thời gian chờ (timeout) 10 giây để tránh treo máy
      await device.connect(autoConnect: false, timeout: const Duration(seconds: 10));
      
      onProgress?.call('Kết nối vật lý thành công! Đang đồng bộ GATT Services...');
      // Khám phá dịch vụ (GATT Discovery) với timeout 10 giây để tránh treo máy
      List<BluetoothService> services = await device.discoverServices().timeout(const Duration(seconds: 10));
      
      BluetoothCharacteristic? hrChar;
      
      onProgress?.call('Đang tìm cổng dịch vụ đo Nhịp tim (0x180D)...');
      for (var service in services) {
        // Tìm service đo nhịp tim: 0x180D
        if (service.uuid.toString().toLowerCase().contains("180d")) {
          for (var char in service.characteristics) {
            // Tìm characteristic nhịp tim đo được: 0x2A37
            if (char.uuid.toString().toLowerCase().contains("2a37")) {
              hrChar = char;
              break;
            }
          }
        }
      }

      if (hrChar == null) {
        throw Exception('Thiết bị không hỗ trợ đặc tính Nhịp tim chuẩn BLE (0x2A37). Hãy thử tắt/bật lại Phát nhịp tim.');
      }

      onProgress?.call('Đang kích hoạt nhận thông báo Nhịp tim real-time...');
      // Kích hoạt nhận thông báo thay đổi (Notifications/Indications) với timeout 5 giây
      await hrChar.setNotifyValue(true).timeout(const Duration(seconds: 5));

      onProgress?.call('Đăng ký luồng dữ liệu nhịp tim thành công!');
      // Đăng ký lắng nghe luồng nhịp tim thay đổi
      _hrValueSubscription = hrChar.lastValueStream.listen((value) {
        int hr = _parseHeartRate(value);
        if (hr > 0) {
          _hrController.add(hr);
        }
      });

    } catch (e) {
      print('Lỗi kết nối BLE: $e');
      await disconnectDevice();
      rethrow;
    }
  }

  /// Ngắt kết nối thiết bị đang kết nối
  Future<void> disconnectDevice() async {
    try {
      if (_connectedDevice != null) {
        await _connectedDevice!.disconnect();
      }
    } catch (_) {} finally {
      _cleanUpConnection();
    }
  }

  /// Làm sạch các luồng lắng nghe khi ngắt kết nối
  void _cleanUpConnection() {
    _hrValueSubscription?.cancel();
    _connStateSubscription?.cancel();
    _hrValueSubscription = null;
    _connStateSubscription = null;
    _connectedDevice = null;
    _connStateController.add(BluetoothConnectionState.disconnected);
  }

  /// Hàm phân tích (parse) giá trị byte nhịp tim nhận được từ BLE GATT 0x2A37
  int _parseHeartRate(List<int> value) {
    if (value.isEmpty) return 0;
    
    int flags = value[0];
    
    // Kiểm tra định dạng nhịp tim (Bit 0 của Flags: 0 = 8-bit, 1 = 16-bit)
    bool is16Bit = (flags & 0x01) != 0;
    
    if (is16Bit) {
      if (value.length < 3) return 0;
      return (value[2] << 8) + value[1];
    } else {
      if (value.length < 2) return 0;
      return value[1];
    }
  }
}
