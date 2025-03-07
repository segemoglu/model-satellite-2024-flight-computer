#BİSMİLLAHİRRAHMANİRRAHİM
#KALİTE TESADÜF DEĞİLDİR
#İKRAM FATİH ÖZTIRPAN
#MAŞAALLAH
import smbus
import time
import threading
from adafruit_pcf8523.pcf8523 import PCF8523
import board
import busio
import subprocess
import serial
import pynmea2

from pynput import keyboard
from Adafruit_BNO055 import BNO055
from datetime import datetime
import socket
import struct

import RPi.GPIO as GPIO

import io
import picamera
import logging
import socketserver
from threading import Condition
from http import server


global_rhrh = "RHRH"

class global_degiskenler():
    global_statu = 0
    global_hata1 ="0"
    global_hata2 ="0"
    global_hata3 ="0"
    global_hata4 ="0"
    global_hata5 ="0"
    global_hata_kodu=global_hata1+global_hata2+global_hata3+global_hata4+global_hata5

    def up():
        global_degiskenler.global_hata_kodu=global_degiskenler.global_hata1+global_degiskenler.global_hata2+global_degiskenler.global_hata3+global_degiskenler.global_hata4+global_degiskenler.global_hata5

def timeAdjust():
    # Şu anki tarih ve saat
    now = datetime.now()

    # Özelleştirilmiş format
    formatted_now = now.strftime("%Y/%m/%d,%H/%M/%S")
    #print("Şu anki tarih ve saat (formatlı):", formatted_now)

    return formatted_now


# RTCClock sınıfı
class RTCClock:
    def __init__(self, i2c):
        self.rtc = PCF8523(i2c)
    
    def set_datetime(self, dt=None):
        if dt is None:
            dt = time.localtime()
        self.rtc.datetime = dt
    
    def get_datetime(self):
        return self.rtc.datetime
    
    def print_time(self):
        t = self.get_datetime()
        print(t)
        print(t.tm_hour, t.tm_min, t.tm_sec)

    def set_system_time(self):
        t = self.get_datetime()

        date_str = time.strftime('%Y-%m-%d', t)
        time_str = time.strftime('%H:%M:%S', t)
        
        subprocess.run(['sudo', 'date', '-s', f'{date_str} {time_str}'], check=True)  # Raspberry Pi'nin saatini ayarla
        #subprocess.run(['sudo', 'hwclock', '-w'], check=True) #RTC saati ayarlama


# I2CReader sınıfı
class I2CReader:
    def __init__(self, slave_address=0x55, bus_number=1):
        self.bus = smbus.SMBus(bus_number)
        self.slave_address = slave_address
        self.last_valid_data = ""
        self.running = True
        self.thread = None

        self.pressure = None
        self.altitude = None
        self.voltage = None

        self.start()

    def read_i2c_data(self):
        try:
            data = self.bus.read_i2c_block_data(self.slave_address, 0, 32)
            data_str = ''.join(chr(i) for i in data if i != 0)
            return data_str
        except OSError as e:
            print(f"Error reading I2C data: {e}")
            return None

    def clean_data(self, data):
        cleaned_data = data.split('ÿ')[0]
        return cleaned_data.strip()

    def is_valid_data(self, data):
        parts = data.split(',')
        if len(parts) == 3:
            try:
                float(parts[0].strip())
                float(parts[1].strip())
                float(parts[2].strip())
                return True
            except ValueError:
                return False
        return False
    
    def run(self):
        while self.running:
            data = self.read_i2c_data()
            if data:
                global_degiskenler.global_hata3="0"
                global_degiskenler.up()
                cleaned_data = self.clean_data(data)
                if self.is_valid_data(cleaned_data):
                    self.last_valid_data = cleaned_data
                    parsed_data = self.parse_data(cleaned_data)
                    self.pressure, self.altitude, self.voltage = map(float, parsed_data)
                else:
                    global_degiskenler.global_hata3="1"
                    global_degiskenler.up()
                    if self.last_valid_data:
                        print("Using last valid data: ", self.last_valid_data)
                        parsed_data = self.parse_data(self.last_valid_data)
                        self.pressure, self.altitude, self.voltage = map(float, parsed_data)
                    else:
                        print("Using last valid data: There is no valid data")
            else:
                global_degiskenler.global_hata3="1"
                global_degiskenler.up()
                print("No data received or error occurred")
                if self.last_valid_data:
                    print("Using last valid data: ", self.last_valid_data)
                    parsed_data = self.parse_data(self.last_valid_data)
                    self.pressure, self.altitude, self.voltage = map(float, parsed_data)
            time.sleep(1)

    def parse_data(self, data):
        parts = data.split(',')
        parsed_data = [part.strip() for part in parts]
        return parsed_data

    def stop(self):
        self.running = False
        if self.thread:
            self.thread.join()

    def start(self):
        self.thread = threading.Thread(target=self.run)
        self.thread.start()

class GPSModule:
    def __init__(self, port="/dev/ttyAMA0", baudrate=9600):
        self.port = port
        self.baudrate = baudrate
        self.ser = serial.Serial(self.port, baudrate=self.baudrate, timeout=1)
        self.location = {'latitude': 0.0, 'longitude': 0.0, 'altitude': 0.0}
        self._running = True

        # GPS verilerini okumaya başla
        self.thread = threading.Thread(target=self._update_location)
        self.thread.start()

    def _update_location(self):
        while self._running:
            try:
                line = self.ser.readline().decode('ascii', errors='replace')
                if line.startswith('$GPGGA'):
                    global_degiskenler.global_hata4="0"
                    global_degiskenler.up()
                    msg = pynmea2.parse(line)
                    if msg.latitude is not None:
                        latitude = msg.latitude  
                    else: 
                        latitude = 0.0
                        global_degiskenler.global_hata4="1"
                        global_degiskenler.up()
                    
                    if msg.longitude is not None:
                        longitude = msg.longitude  
                    else: 
                        longitude = 0.0
                        global_degiskenler.global_hata4="1"
                        global_degiskenler.up()
                    
                    if msg.altitude is not None:
                        altitude = msg.altitude  
                    else: 
                        altitude = 0.0
                        global_degiskenler.global_hata4="1"
                        global_degiskenler.up()


                    self.location = {
                        'latitude': latitude,
                        'longitude': longitude,
                        'altitude': altitude
                    }
            except Exception as e:
                global_degiskenler.global_hata4="1"
                global_degiskenler.up()
                print(f"Error reading GPS data: {e}")

    def get_location(self):
        return self.location

    def stop(self):
        self._running = False
        if self.thread:
            self.thread.join()


# class GPSModule:
#     def __init__(self, port="/dev/ttyAMA0", baudrate=9600):
#         self.port = port
#         self.baudrate = baudrate
#         self.ser = serial.Serial(self.port, baudrate=self.baudrate, timeout=1)
#         self.location = None
#         self._running = True


#         self.thread = threading.Thread(target=self._update_location)
#         self.thread.start()

#     def _update_location(self):
#         while self._running:
#             try:
#                 line = self.ser.readline().decode('ascii', errors='replace')
#                 if line.startswith('$GPGGA'):
#                     global hata4
#                     hata4="0"
#                     msg = pynmea2.parse(line)
#                     if msg.latitude is not None:
#                         latitude = msg.latitude  
#                     else: 
#                         latitude = 0.0
                    
#                     if msg.longitude is not None:
#                         longitude = msg.longitude  
#                     else: 
#                         longitude = 0.0

#                     if msg.altitude is not None:
#                         altitude = msg.altitude  
#                     else: 
#                         altitude = 0.0
#                     self.location = {
#                         'latitude': latitude,
#                         'longitude': longitude,
#                         'altitude': altitude
#                     }
#             except Exception as e:
#                 hata4="1"
#                 print(f"Error reading GPS data: {e}")

#     def get_location(self):
#         return self.location

#     def stop(self):
#         self._running = False
#         if self.thread:
#             self.thread.join()

import smbus
import threading
import time
from datetime import datetime

class MS5611App:
    MS5611_ADDRESS = 0x77
    CMD_RESET = 0x1E
    CMD_CONV_D1 = 0x48  # Basınç dönüşümü
    CMD_CONV_D2 = 0x58  # Sıcaklık dönüşümü
    CMD_ADC_READ = 0x00
    CMD_PROM_READ = 0xA2
    STANDARD_PRESSURE = 101325

    def __init__(self, bus_num=1):
        self.bus = smbus.SMBus(bus_num)
        self.lock = threading.Lock()
        
        # Sensörü sıfırla ve kalibrasyon verilerini oku
        self.reset()
        self.calibration = self.read_calibration_data()
        self.altitude_offset = 0
        self.previous_altitude = None
        self.previous_time = None
        self.sensor_data = None

        # İlk yükseklik verisini al ve ofset olarak ayarla
        self.initialize_altitude_offset()

        # Thread başlatma
        self.running = True
        self.thread = threading.Thread(target=self.update_sensor_data)
        self.thread.start()

    def reset(self):
        with self.lock:
            self.bus.write_byte(self.MS5611_ADDRESS, self.CMD_RESET)
            time.sleep(0.1)

    def read_calibration_data(self):
        calibration = []
        with self.lock:
            for i in range(6):
                data = self.bus.read_i2c_block_data(self.MS5611_ADDRESS, self.CMD_PROM_READ + (i * 2), 2)
                calibration.append(data[0] << 8 | data[1])
        return calibration

    def start_conversion(self, command):
        with self.lock:
            self.bus.write_byte(self.MS5611_ADDRESS, command)
            time.sleep(0.01)  # Dönüşüm süresi

    def read_adc(self):
        with self.lock:
            data = self.bus.read_i2c_block_data(self.MS5611_ADDRESS, self.CMD_ADC_READ, 3)
        return data[0] << 16 | data[1] << 8 | data[2]

    def read_temperature_and_pressure(self):
        self.start_conversion(self.CMD_CONV_D1)
        time.sleep(0.01)  # Basınç dönüşüm süresi
        D1 = self.read_adc()

        self.start_conversion(self.CMD_CONV_D2)
        time.sleep(0.01)  # Sıcaklık dönüşüm süresi
        D2 = self.read_adc()

        C1, C2, C3, C4, C5, C6 = self.calibration
        dT = D2 - C5 * 256
        TEMP = 2000 + dT * C6 / 8388608

        OFF = C2 * 65536 + (C4 * dT) / 128
        SENS = C1 * 32768 + (C3 * dT) / 256
        P = (D1 * SENS / 2097152 - OFF) / 32768

        if TEMP < 2000:
            T2 = dT * dT / 2147483648
            OFF2 = 5 * ((TEMP - 2000) * (TEMP - 2000)) / 2
            SENS2 = 5 * ((TEMP - 2000) * (TEMP - 2000)) / 4
            if TEMP < -1500:
                OFF2 += 7 * ((TEMP + 1500) * (TEMP + 1500))
                SENS2 += 11 * ((TEMP + 1500) * (TEMP + 1500)) / 2
            TEMP -= T2
            OFF -= OFF2
            SENS -= SENS2

        P = (D1 * SENS / 2097152 - OFF) / 32768
        return TEMP / 100.0, P / 100.0

    def calculate_altitude(self, pressure):
        P_Pa = pressure * 100
        altitude = 44330 * (1 - (P_Pa / self.STANDARD_PRESSURE) ** 0.1903)
        return altitude

    def set_altitude_offset(self, offset):
        with self.lock:
            self.altitude_offset = offset

    def get_adjusted_altitude(self, pressure):
        raw_altitude = self.calculate_altitude(pressure)
        adjusted_altitude = raw_altitude - self.altitude_offset
        return adjusted_altitude

    def read_sensor_data(self):
        temperature, pressure = self.read_temperature_and_pressure()
        altitude = self.get_adjusted_altitude(pressure)
        current_time = time.time()
        
        # Dikey hız hesaplama
        vertical_speed = 0
        if self.previous_altitude is not None and self.previous_time is not None:
            delta_altitude = altitude - self.previous_altitude
            delta_time = current_time - self.previous_time
            vertical_speed = delta_altitude / delta_time

        self.previous_altitude = altitude
        self.previous_time = current_time
        
        now = datetime.now()
        timestamp = now.strftime('%S.%f')[:-3]

        with self.lock:
            self.sensor_data = {
                "timestamp": timestamp,
                "temperature": temperature,
                "pressure": pressure,
                "altitude": altitude,
                "vertical_speed": vertical_speed
            }

    def update_sensor_data(self):
        while self.running:
            self.read_sensor_data()
            time.sleep(0.25)  # Veriyi her 0.25 saniyede bir güncelle

    def get_sensor_data(self):
        with self.lock:
            return self.sensor_data

    def initialize_altitude_offset(self):
        # İlk yüksekliği hesapla ve ofset olarak ayarla
        temperature, pressure = self.read_temperature_and_pressure()
        initial_altitude = self.calculate_altitude(pressure)
        self.set_altitude_offset(initial_altitude)
        self.previous_altitude = initial_altitude
        self.previous_time = time.time()
        #print("İlk yükseklik ofset olarak ayarlandı.")

    def stop(self):
        self.running = False
        self.thread.join()


class BNO055App:
    def __init__(self):
        self.bno = BNO055.BNO055()
        self._orientation_data = None  # Başlangıçta orientation_data'yı None olarak tanımla
        self.running = False
        self.lock = threading.Lock()  # Veri güvenliği için bir kilit tanımla

        if not self.bno.begin():
            raise RuntimeError('BNO055 sensörü bulunamadı, doğru I2C adresini kullanıyor musunuz?')

        # Thread'i başlatmak için initialize sırasında start_reading'i çağır
        self.start_reading()

    def read_orientation(self):
        while self.running:
            # Yaw, Roll ve Pitch değerlerini oku
            heading, roll, pitch = self.bno.read_euler()
            # Zaman damgası al
            now = datetime.now()
            timestamp = now.strftime('%S.%f')[:-3]  # Saniye ve milisaniye
            # Değerleri sakla
            with self.lock:
                self._orientation_data = (timestamp, heading, roll, pitch)
            # 0.1 saniye bekle
            time.sleep(0.1)

    def start_reading(self):
        self.running = True
        self.thread = threading.Thread(target=self.read_orientation)
        self.thread.start()

    def stop_reading(self):
        self.running = False
        self.thread.join()

    def get_orientation(self):
        with self.lock:
            return self._orientation_data
        

class Client:
    def __init__(self, host, port):
        self.host = host
        self.port = port
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.thread2 = False
        self.thread = False
        self.packetSent=0

        self.i1= None
        self.i2=None
        self.s1=None
        self.s2= None
        self.f1 =None
        self.f2=None
        self.f3=None
        self.f4=None
        self.f5= None
        self.f6=None
        self.f7=None
        self.f8=None
        self.f9=None
        self.f10=None
        self.f11=None
        self.f12=None
        self.f13=None
        self.f14=None
        self.s3=None
        self.f15=None
        self.i3=None




    def connect(self):
        self.sock.connect((self.host, self.port))
        print("Bağlandı")

    def send_ints(self, *ints):
        packed_data = struct.pack(f'{len(ints)}i', *ints)
        self.sock.sendall(packed_data)

    def send_string(self, text):
        text_encoded = text.encode('utf-8')
        self.sock.sendall(struct.pack('I', len(text_encoded)))
        self.sock.sendall(text_encoded)

    def send_floats(self, *floats):
        packed_data = struct.pack(f'{len(floats)}f', *floats)
        self.sock.sendall(packed_data)

    def run(self):
        self.connect()
        while True:
            self.send_ints(self.i1, self.i2)
            self.send_string(self.s1)
            self.send_string(self.s2)
            self.send_floats(self.f1, self.f2, self.f3, self.f4, self.f5, self.f6, self.f7, self.f8, self.f9, 
                             self.f10, self.f11, self.f12, self.f13, self.f14)
            self.send_string(self.s3)
            self.send_floats(self.f15)
            self.send_ints(self.i3)

            self.packetSent+=1
            time.sleep(1)

    def start(self, int1, int2, s1, s2, f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14, s3, f15, i4):
        self.i1= int1
        self.i2=int2
        self.s1=s1
        self.s2= s2
        self.f1 =f1
        self.f2=f2
        self.f3=f3
        self.f4=f4
        self.f5=f5
        self.f6=f6
        self.f7=f7
        self.f8=f8
        self.f9=f9
        self.f10=f10
        self.f11=f11
        self.f12=f12
        self.f13=f13
        self.f14=f14
        self.s3=s3
        self.f15=f15
        self.i3=i4
        if self.thread==False:
            threading.Thread(target=self.run, daemon=True).start()
            self.thread = True

class DataProcessor:
    def __init__(self, i2c_reader, rtc_clock, gps_module, ms5611_app, bno055_app, client):
        self.i2c_reader = i2c_reader
        self.rtc_clock = rtc_clock
        self.gps_module = gps_module
        self.ms5611_app = ms5611_app
        self.bno055_app = bno055_app
        self.client = client
        self.pressure_data = None
        self.altitude_data = None
        self.voltage_data = None
        self.current_time = None
        self.gps_location = None
        self.ms5611_data = None
        self.bno055_data = None
        self.running = True
        self.thread = None

    def update_data(self):
        self.pressure_data = self.i2c_reader.pressure
        self.altitude_data = self.i2c_reader.altitude
        self.voltage_data = self.i2c_reader.voltage
        self.current_time = self.rtc_clock.get_datetime()
        self.gps_location = self.gps_module.get_location()
        self.ms5611_data = self.ms5611_app.get_sensor_data()
        self.bno055_data = self.bno055_app.get_orientation()

    def display_data(self):
        if self.pressure_data is not None:
            print(f"Pressure (I2CReader): {self.pressure_data} Pa")
        if self.altitude_data is not None:
            print(f"Altitude (I2CReader): {self.altitude_data} meters")
        if self.voltage_data is not None:
            print(f"Voltage (I2CReader): {self.voltage_data} V")

        if self.current_time is not None:
            try:
                print("Current Time:", time.strftime('%Y-%m-%d %H:%M:%S', self.current_time))
            except ValueError as e:
                print(f"Invalid time data received: {self.current_time}, Error: {e}")

        if self.gps_location is not None:
            print(f"GPS Location: Latitude: {self.gps_location['latitude']}, Longitude: {self.gps_location['longitude']}, Altitude: {self.gps_location['altitude']} meters")
        else:
            print("GPS Location: No data available")

        if self.ms5611_data is not None:
            print(f"MS5611 Data: Timestamp: {self.ms5611_data['timestamp']}, Temperature: {self.ms5611_data['temperature']} C, Pressure: {self.ms5611_data['pressure']} hPa, Altitude: {self.ms5611_data['altitude']} meters, Vertical Speed: {self.ms5611_data['vertical_speed']} m/s")
        else:
            print("MS5611 Data: No data available")

        if self.bno055_data is not None:
            timestamp, heading, roll, pitch = self.bno055_data
            print(f"BNO055 Data: Timestamp: {timestamp}, Heading: {heading} degrees, Roll: {roll} degrees, Pitch: {pitch} degrees")
        else:
            print("BNO055 Data: No data available")

    def send_data_to_client(self):
            self.client.start(self.client.packetSent+1,
                              global_degiskenler.global_statu,
                              global_degiskenler.global_hata_kodu,
                              timeAdjust(),
                              self.ms5611_data['pressure'],
                              self.pressure_data,
                              self.ms5611_data['altitude'],
                              self.altitude_data,
                              abs(self.ms5611_data['altitude'] - self.altitude_data),
                              self.ms5611_data['vertical_speed'],
                              self.ms5611_data['temperature'],
                              self.voltage_data,
                              self.gps_location['latitude'],
                              self.gps_location['longitude'],
                              self.gps_location['altitude'],
                              self.bno055_data[3],
                              self.bno055_data[2],
                              self.bno055_data[1],
                              global_rhrh,
                              1903,
                              270942)
            
            if (global_degiskenler.global_statu==2):
                if (self.ms5611_data['vertical_speed']>12 and self.ms5611_data['vertical_speed']<14):
                    global_degiskenler.global_hata1="0"
                    global_degiskenler.up()
                else:
                    global_degiskenler.global_hata1="1"
                    global_degiskenler.up()

            if (global_degiskenler.global_statu==4):
                if (self.ms5611_data['vertical_speed']>6 and self.ms5611_data['vertical_speed']<8):
                    global_degiskenler.global_hata2="0"
                    global_degiskenler.up()
                else:
                    global_degiskenler.global_hata2="1"
                    global_degiskenler.up()


    def receive_data_from_client(self):
        self.client.start2()

    def run(self):
        while self.running:
            self.update_data()
            #self.display_data()
            self.send_data_to_client()  # Verileri client'a gönder
            #self.receive_data_from_client()
            time.sleep(1)

    def start(self):
        self.thread = threading.Thread(target=self.run)
        self.thread.start()

    def stop(self):
        self.running = False
        if self.thread:
            self.thread.join()
        self.gps_module.stop()
        self.ms5611_app.stop()
        self.bno055_app.stop_reading()
        # Eğer stop() metodu Client sınıfında varsa, kullanabilirsiniz.
        # self.client.stop()

class ServoController:
    def __init__(self, servo_pin1, servo_pin2, angles, start_angle=0):
        self.servo_pin1 = servo_pin1
        self.servo_pin2 = servo_pin2
        self.angles = angles
        self.start_angle = start_angle

        GPIO.setmode(GPIO.BCM)
        GPIO.setup(self.servo_pin1, GPIO.OUT)
        GPIO.setup(self.servo_pin2, GPIO.OUT)

        self.pwm1 = GPIO.PWM(self.servo_pin1, 50)  # 50Hz PWM frekansı
        self.pwm2 = GPIO.PWM(self.servo_pin2, 50)

        self.pwm1.start(0)
        self.pwm2.start(0)

    def set_servo_angle(self, servo, angle):
        duty = angle / 18 + 2
        GPIO.output(servo, True)
        if servo == self.servo_pin1:
            self.pwm1.ChangeDutyCycle(duty)
        elif servo == self.servo_pin2:
            self.pwm2.ChangeDutyCycle(duty)
        time.sleep(1)
        GPIO.output(servo, False)
        if servo == self.servo_pin1:
            self.pwm1.ChangeDutyCycle(0)
        elif servo == self.servo_pin2:
            self.pwm2.ChangeDutyCycle(0)

    def is_valid_command(self, command):
        if len(command) != 4:
            return False
        if not (command[0].isdigit() and command[1].isalpha() and command[2].isdigit() and command[3].isalpha()):
            return False
        if not (1 <= int(command[0]) <= 9 and 1 <= int(command[2]) <= 9):
            return False
        if not (command[1] in self.angles and command[3] in self.angles):
            return False
        return True

    def execute_command(self, command):
        global global_rhrh
        global_rhrh = command


        first_part = command[:2]
        second_part = command[2:]
        
        first_angle = self.angles[first_part[1]]
        first_time = int(first_part[0])
        
        second_angle = self.angles[second_part[1]]
        second_time = int(second_part[0])
       
        self.pwm2.ChangeDutyCycle(2 + (first_angle / 27))
        time.sleep(first_time)
        
        self.pwm2.ChangeDutyCycle(2 + (second_angle / 27))
        time.sleep(second_time)
        
        self.pwm2.ChangeDutyCycle(2 + (self.start_angle / 27))
        time.sleep(1)
        
        self.pwm2.ChangeDutyCycle(0)

        return command  # Komutun döndürülmesi
        

    def cleanup(self):
        self.pwm1.stop()
        self.pwm2.stop()
        GPIO.cleanup()

class TCPServer:
    def __init__(self, host, port, servo_controller):
        self.host = host
        self.port = port
        self.servo_controller = servo_controller

        self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server_socket.bind((self.host, self.port))
        self.server_socket.listen(1)
        print("Sunucu başlatıldı. Bağlantı bekleniyor...")

    def handle_client(self, client_socket):
        data = client_socket.recv(1024).decode('utf-8')
        command = None

        if data == '180':
            self.servo_controller.set_servo_angle(self.servo_controller.servo_pin1, 180)
        elif data == '80':
            self.servo_controller.set_servo_angle(self.servo_controller.servo_pin1, 80)
        elif self.servo_controller.is_valid_command(data):
            command = self.servo_controller.execute_command(data)
            response = "Komut başarıyla tamamlandı."
            client_socket.send(response.encode('utf-8'))
        else:
            response = "Hatalı komut."
            client_socket.send(response.encode('utf-8'))

        client_socket.close()
        global global_rhrh
        global_rhrh = "RHRH"
        return command  # Komutun döndürülmesi

    def start(self):
        while True:
            client_socket, address = self.server_socket.accept()
            print(f"Bağlantı kabul edildi: {address}")
            command = self.handle_client(client_socket)
            # Burada sadece döndürülen komutu kullanabilirsiniz
            if command:
                print(command)
                pass

    def cleanup(self):
        self.server_socket.close()

def servosss(tcp):
    try:
        tcp_server.start()
        global_degiskenler.global_hata5="0"
        global_degiskenler.up()
    except KeyboardInterrupt:
        global_degiskenler.global_hata5="1"
        global_degiskenler.up()
        print("Program sonlandırılıyor...")
    finally:
        tcp_server.cleanup()
        servo_controller.cleanup()

client_app = Client("192.168.218.26", 12345)
i2c_reader = I2CReader()
ms5611_app = MS5611App()
bno055_app = BNO055App()

i2c = busio.I2C(board.SCL, board.SDA)
rtc_clock = RTCClock(i2c)  
gps_module = GPSModule()

data_processor = DataProcessor(i2c_reader, rtc_clock, gps_module, ms5611_app, bno055_app, client_app)
data_processor.start()

#data_processor.stop()

# Servo ve sunucu ayarları
angles = {"R": 90, "G": 180, "B": 270, "r": 90, "g": 180, "b": 270}
servo_controller = ServoController(servo_pin1=27, servo_pin2=17, angles=angles)
tcp_server = TCPServer(host='0.0.0.0', port=65432, servo_controller=servo_controller)

server_thread = threading.Thread(target=servosss, args=(tcp_server,))
server_thread.start()





PAGE = """\
<html>
<head>
<title>Raspberry Pi - MJPEG Stream</title>
</head>
<body>
<img src="stream.mjpg" width="1280" height="720" />
</body>
</html>
"""

class StreamingOutput(object):
    def __init__(self):
        self.frame = None
        self.buffer = io.BytesIO()
        self.condition = Condition()

    def write(self, buf):
        if buf.startswith(b'\xff\xd8'):
            # Başlangıçtan itibaren yeni bir kare alındığında
            # Önceki kareyi yayınla
            with self.condition:
                self.frame = self.buffer.getvalue()
                self.buffer.truncate(0)
                self.buffer.seek(0)
                self.condition.notify_all()
        return self.buffer.write(buf)

class StreamingHandler(server.BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path == '/':
            self.send_response(301)
            self.send_header('Location', '/index.html')
            self.end_headers()
        elif self.path == '/index.html':
            content = PAGE.encode('utf-8')
            self.send_response(200)
            self.send_header('Content-Type', 'text/html')
            self.send_header('Content-Length', len(content))
            self.end_headers()
            self.wfile.write(content)
        elif self.path == '/stream.mjpg':
            self.send_response(200)
            self.send_header('Content-Type', 'multipart/x-mixed-replace; boundary=frame')
            self.end_headers()
            try:
                while True:
                    with output.condition:
                        output.condition.wait()
                        frame = output.frame
                    self.wfile.write(b'--frame\r\n')
                    self.send_header('Content-Type', 'image/jpeg')
                    self.send_header('Content-Length', len(frame))
                    self.end_headers()
                    self.wfile.write(frame)
                    self.wfile.write(b'\r\n')
            except Exception as e:
                logging.warning(
                    'Hata: %s',
                    str(e)
                )
        else:
            self.send_error(404)
            self.end_headers()

class StreamingServer(socketserver.ThreadingMixIn, server.HTTPServer):
    allow_reuse_address = True
    daemon_threads = True




with picamera.PiCamera(resolution='1280x720', framerate=30) as camera:
    output = StreamingOutput()
    
    # Video kaydetme işlemi için dosya ismi oluşturma
    video_filename = f'/home/hp/Desktop/video_{datetime.now().strftime("%Y%m%d_%H%M%S")}.h264'
    
    camera.start_recording(output, format='mjpeg')
    camera.start_recording(video_filename, format='h264', splitter_port=2)
    try:
        address = ('', 8000)
        server = StreamingServer(address, StreamingHandler)
        server.serve_forever()
    finally:
        camera.stop_recording(splitter_port=2)
        camera.stop_recording()
