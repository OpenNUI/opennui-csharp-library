This README is just a fast quick start document. You can find more detailed documentation at http://www.opennui.org

# OpenNUI C# Library
OpenNUI Framework의 C# 어플리케이션을 개발할 때 사용하는 Library입니다. 
한번의 빌드로 다양한 NUI 모션인식 센서를 지원할 수 있습니다.

현재 지원하는 센서는 다음과 같습니다:
- Kinect1
- Kinect2
- Intel Realsense

# Quick Start
아래와 같은 코드 입력으로 다양한 센서로부터 컬러 프레임 이미지를 가져올 수 있습니다.

```csharp
NuiApplication App = new NuiApplication("TestApp");
App.OnLoad += () {
  App.OnSensorConnected += (NuiSensor sensor) {
    sensor.OpenColorFrame();
    ImageData image = sensor.GetColorFrame();
    // image.FrameData is Color Frame Array of Byte. 
  }
}
App.Start();
```
