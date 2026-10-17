"""
AnFit Python Microservice — Garmin Bridge + AI Engine
Cầu nối giữa Garmin Cloud và AnFit C# API.
Đồng thời cung cấp AI Inference API (User Profiler + Exercise Recommender).
"""

import os
from datetime import date
from contextlib import asynccontextmanager

from fastapi import FastAPI, HTTPException
from garminconnect import Garmin
from dotenv import load_dotenv

# Load .env TRƯỚC khi import ai_router (vì ai_router đọc GEMINI_API_KEY lúc import)
load_dotenv()

from ai_router import ai_router
GARMIN_EMAIL = os.getenv("GARMIN_EMAIL")
GARMIN_PASSWORD = os.getenv("GARMIN_PASSWORD")

# Dictionary lưu cache các kết nối Garmin đang hoạt động theo email
import hashlib
garmin_clients: dict[str, Garmin] = {}


def get_garmin_client(email: str | None = None, password: str | None = None) -> Garmin:
    """Khởi tạo và cache phiên đăng nhập Garmin của từng user. Dùng token lưu sẵn để tránh 429."""
    global garmin_clients
    
    # Dùng email/password mặc định từ .env nếu không truyền vào
    u_email = (email or GARMIN_EMAIL or "").strip().lower()
    u_password = password or GARMIN_PASSWORD
    
    if not u_email or not u_password:
        raise HTTPException(
            status_code=400,
            detail="Thiếu thông tin đăng nhập Garmin (Email/Password)."
        )
    
    # Nếu đã có client trong cache
    if u_email in garmin_clients:
        return garmin_clients[u_email]
        
    # Tạo thư mục lưu token riêng biệt cho từng email để tránh xung đột
    email_hash = hashlib.md5(u_email.encode()).hexdigest()
    token_dir = f"./garmin_tokens/{email_hash}"
    
    client = Garmin(u_email, u_password)
    
    try:
        # Thử load token đã lưu (phiên đăng nhập cũ)
        print(f"Đang thử đăng nhập bằng token cũ cho {u_email}...")
        client.login(token_dir)
        print(f"Đăng nhập Garmin bằng token cũ cho {u_email} thành công!")
    except Exception:
        # Nếu token cũ lỗi hoặc chưa có, đăng nhập mới
        print(f"Không có token hợp lệ cho {u_email}. Đang tiến hành đăng nhập mới...")
        try:
            client.login()
            # Lưu token lại cho những lần sau
            os.makedirs(token_dir, exist_ok=True)
            client.garth.dump(token_dir)
            print(f"Đăng nhập mới thành công và đã lưu token cho {u_email}!")
        except Exception as e:
            raise HTTPException(
                status_code=401,
                detail=f"Không thể kết nối Garmin Cloud: {str(e)}"
            )
            
    garmin_clients[u_email] = client
    return client


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Tự động load AI Models khi khởi động server."""
    print("[START] AnFit Python Microservice dang khoi dong...")
    
    # Khởi động Garmin Bridge bằng tài khoản mặc định nếu cấu hình sẵn
    if GARMIN_EMAIL and GARMIN_PASSWORD:
        try:
            get_garmin_client()
            print("[OK] Dang nhap Garmin Cloud mac dinh thanh cong!")
        except Exception as e:
            print(f"[WARN] Chua dang nhap duoc Garmin mac dinh: {e}")
    else:
        print("[INFO] Chua cau hinh tai khoan mac dinh. Cau noi hoat dong che do Dang nhap Dong.")
        
    print("[AI] AI Engine san sang.")
    yield
    print("[STOP] AnFit Python Microservice tat.")


app = FastAPI(
    title="AnFit Python Microservice",
    description="Garmin Bridge + AI Engine (User Profiler, Exercise Recommender)",
    version="2.0.0",
    lifespan=lifespan
)

# Gộp AI Router vào cùng app
app.include_router(ai_router)


from fastapi import Header

@app.get("/api/health/daily/{target_date}")
async def get_daily_summary(
    target_date: str,
    x_garmin_email: str | None = Header(None, alias="X-Garmin-Email"),
    x_garmin_password: str | None = Header(None, alias="X-Garmin-Password")
):
    """
    Lấy dữ liệu sức khỏe tổng hợp trong ngày từ Garmin của từng user cụ thể.
    """
    u_email = (x_garmin_email or "").strip().lower()
    try:
        client = get_garmin_client(u_email, x_garmin_password)
        stats = client.get_stats(target_date)

        # Cố gắng lấy thêm dữ liệu giấc ngủ
        sleep_minutes = 0
        try:
            sleep = client.get_sleep_data(target_date)
            sleep_seconds = sleep.get("dailySleepDTO", {}).get("sleepTimeSeconds", 0) or 0
            sleep_minutes = sleep_seconds // 60
        except Exception:
            pass 

        normalized = {
            "activeKilocalories": stats.get("activeKilocalories", 0) or 0,
            "consumedKilocalories": stats.get("consumedKilocalories", 0) or 0,
            "steps": stats.get("totalSteps", 0) or 0,
            "restingHeartRate": stats.get("restingHeartRate", 0) or 0,
            "maxHeartRate": stats.get("maxHeartRate", 0) or 0,
            "stressLevel": stats.get("averageStressLevel", 0) or 0,
            "sleepDurationMinutes": sleep_minutes
        }

        return [normalized]

    except HTTPException:
        raise
    except Exception as e:
        # Session hết hạn → xóa cache đăng nhập để thử lại 1 lần
        global garmin_clients
        if u_email:
            garmin_clients.pop(u_email, None)
            
        try:
            client = get_garmin_client(u_email, x_garmin_password)
            stats = client.get_stats(target_date)

            sleep_minutes = 0
            try:
                sleep = client.get_sleep_data(target_date)
                sleep_seconds = sleep.get("dailySleepDTO", {}).get("sleepTimeSeconds", 0) or 0
                sleep_minutes = sleep_seconds // 60
            except Exception:
                pass

            normalized = {
                "activeKilocalories": stats.get("activeKilocalories", 0) or 0,
                "consumedKilocalories": stats.get("consumedKilocalories", 0) or 0,
                "steps": stats.get("totalSteps", 0) or 0,
                "restingHeartRate": stats.get("restingHeartRate", 0) or 0,
                "maxHeartRate": stats.get("maxHeartRate", 0) or 0,
                "stressLevel": stats.get("averageStressLevel", 0) or 0,
                "sleepDurationMinutes": sleep_minutes
            }
            return [normalized]
        except Exception as retry_err:
            raise HTTPException(
                status_code=502,
                detail=f"Lỗi kết nối Garmin Cloud: {str(retry_err)}"
            )


@app.get("/api/health/sleep/{target_date}")
async def get_sleep_data(
    target_date: str,
    x_garmin_email: str | None = Header(None, alias="X-Garmin-Email"),
    x_garmin_password: str | None = Header(None, alias="X-Garmin-Password")
):
    """
    Lấy dữ liệu giấc ngủ trong ngày từ Garmin.
    """
    u_email = (x_garmin_email or "").strip().lower()
    try:
        client = get_garmin_client(u_email, x_garmin_password)
        sleep = client.get_sleep_data(target_date)

        sleep_seconds = sleep.get("dailySleepDTO", {}).get("sleepTimeSeconds", 0) or 0
        sleep_minutes = sleep_seconds // 60

        return {
            "sleepDurationMinutes": sleep_minutes,
            "date": target_date
        }

    except HTTPException:
        raise
    except Exception as e:
        global garmin_clients
        if u_email:
            garmin_clients.pop(u_email, None)
            
        try:
            client = get_garmin_client(u_email, x_garmin_password)
            sleep = client.get_sleep_data(target_date)
            sleep_seconds = sleep.get("dailySleepDTO", {}).get("sleepTimeSeconds", 0) or 0
            sleep_minutes = sleep_seconds // 60
            return {
                "sleepDurationMinutes": sleep_minutes,
                "date": target_date
            }
        except Exception as retry_err:
            raise HTTPException(
                status_code=502,
                detail=f"Lỗi lấy dữ liệu giấc ngủ: {str(retry_err)}"
            )


@app.get("/api/health/status")
async def health_check(
    x_garmin_email: str | None = Header(None, alias="X-Garmin-Email"),
    x_garmin_password: str | None = Header(None, alias="X-Garmin-Password")
):
    """Kiểm tra trạng thái kết nối Garmin."""
    u_email = (x_garmin_email or "").strip().lower()
    if not u_email or not x_garmin_password:
        return {
            "status": "disconnected",
            "message": "Không có thông tin đăng nhập được truyền qua Header."
        }
    try:
        client = get_garmin_client(u_email, x_garmin_password)
        try:
            name = client.get_full_name()
        except Exception:
            name = u_email
        return {
            "status": "connected",
            "email": u_email,
            "name": name,
            "message": "Kết nối Garmin hoạt động bình thường."
        }
    except Exception as e:
        raise HTTPException(
            status_code=401,
            detail=f"Đăng nhập thất bại hoặc lỗi kết nối: {str(e)}"
        )
