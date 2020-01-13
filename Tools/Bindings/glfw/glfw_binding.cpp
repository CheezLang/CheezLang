#include <memory>
#include "glfw_binding_source.cpp"

extern "C" void __c__glfwInit(int32_t *ret) {
    *ret = (int32_t )glfwInit();
}
extern "C" void __c__glfwTerminate() {
    glfwTerminate();
}
extern "C" void __c__glfwInitHint(int32_t _hint, int32_t _value) {
    glfwInitHint(_hint, _value);
}
extern "C" void __c__glfwGetVersion(int32_t * _major, int32_t * _minor, int32_t * _rev) {
    glfwGetVersion(_major, _minor, _rev);
}
extern "C" void __c__glfwGetVersionString(char * *ret) {
    *ret = (char * )glfwGetVersionString();
}
extern "C" void __c__glfwGetError(int32_t *ret, char * * _description) {
    *ret = (int32_t )glfwGetError(_description);
}
extern "C" void __c__glfwSetErrorCallback(GLFWerrorfun *ret, GLFWerrorfun _cbfun) {
    *ret = (GLFWerrorfun )glfwSetErrorCallback(_cbfun);
}
extern "C" void __c__glfwGetMonitors(GLFWmonitor * * *ret, int32_t * _count) {
    *ret = (GLFWmonitor * * )glfwGetMonitors(_count);
}
extern "C" void __c__glfwGetPrimaryMonitor(GLFWmonitor * *ret) {
    *ret = (GLFWmonitor * )glfwGetPrimaryMonitor();
}
extern "C" void __c__glfwGetMonitorPos(GLFWmonitor * _monitor, int32_t * _xpos, int32_t * _ypos) {
    glfwGetMonitorPos(_monitor, _xpos, _ypos);
}
extern "C" void __c__glfwGetMonitorWorkarea(GLFWmonitor * _monitor, int32_t * _xpos, int32_t * _ypos, int32_t * _width, int32_t * _height) {
    glfwGetMonitorWorkarea(_monitor, _xpos, _ypos, _width, _height);
}
extern "C" void __c__glfwGetMonitorPhysicalSize(GLFWmonitor * _monitor, int32_t * _widthMM, int32_t * _heightMM) {
    glfwGetMonitorPhysicalSize(_monitor, _widthMM, _heightMM);
}
extern "C" void __c__glfwGetMonitorContentScale(GLFWmonitor * _monitor, float * _xscale, float * _yscale) {
    glfwGetMonitorContentScale(_monitor, _xscale, _yscale);
}
extern "C" void __c__glfwGetMonitorName(char * *ret, GLFWmonitor * _monitor) {
    *ret = (char * )glfwGetMonitorName(_monitor);
}
extern "C" void __c__glfwSetMonitorUserPointer(GLFWmonitor * _monitor, void * _pointer) {
    glfwSetMonitorUserPointer(_monitor, _pointer);
}
extern "C" void __c__glfwGetMonitorUserPointer(void * *ret, GLFWmonitor * _monitor) {
    *ret = (void * )glfwGetMonitorUserPointer(_monitor);
}
extern "C" void __c__glfwSetMonitorCallback(GLFWmonitorfun *ret, GLFWmonitorfun _cbfun) {
    *ret = (GLFWmonitorfun )glfwSetMonitorCallback(_cbfun);
}
extern "C" void __c__glfwGetVideoModes(const GLFWvidmode * *ret, GLFWmonitor * _monitor, int32_t * _count) {
    *ret = (const GLFWvidmode * )glfwGetVideoModes(_monitor, _count);
}
extern "C" void __c__glfwGetVideoMode(const GLFWvidmode * *ret, GLFWmonitor * _monitor) {
    *ret = (const GLFWvidmode * )glfwGetVideoMode(_monitor);
}
extern "C" void __c__glfwSetGamma(GLFWmonitor * _monitor, float _gamma) {
    glfwSetGamma(_monitor, _gamma);
}
extern "C" void __c__glfwGetGammaRamp(const GLFWgammaramp * *ret, GLFWmonitor * _monitor) {
    *ret = (const GLFWgammaramp * )glfwGetGammaRamp(_monitor);
}
extern "C" void __c__glfwSetGammaRamp(GLFWmonitor * _monitor, const GLFWgammaramp * _ramp) {
    glfwSetGammaRamp(_monitor, _ramp);
}
extern "C" void __c__glfwDefaultWindowHints() {
    glfwDefaultWindowHints();
}
extern "C" void __c__glfwWindowHint(int32_t _hint, int32_t _value) {
    glfwWindowHint(_hint, _value);
}
extern "C" void __c__glfwWindowHintString(int32_t _hint, char * _value) {
    glfwWindowHintString(_hint, _value);
}
extern "C" void __c__glfwCreateWindow(GLFWwindow * *ret, int32_t _width, int32_t _height, char * _title, GLFWmonitor * _monitor, GLFWwindow * _share) {
    *ret = (GLFWwindow * )glfwCreateWindow(_width, _height, _title, _monitor, _share);
}
extern "C" void __c__glfwDestroyWindow(GLFWwindow * _window) {
    glfwDestroyWindow(_window);
}
extern "C" void __c__glfwWindowShouldClose(int32_t *ret, GLFWwindow * _window) {
    *ret = (int32_t )glfwWindowShouldClose(_window);
}
extern "C" void __c__glfwSetWindowShouldClose(GLFWwindow * _window, int32_t _value) {
    glfwSetWindowShouldClose(_window, _value);
}
extern "C" void __c__glfwSetWindowTitle(GLFWwindow * _window, char * _title) {
    glfwSetWindowTitle(_window, _title);
}
extern "C" void __c__glfwSetWindowIcon(GLFWwindow * _window, int32_t _count, const GLFWimage * _images) {
    glfwSetWindowIcon(_window, _count, _images);
}
extern "C" void __c__glfwGetWindowPos(GLFWwindow * _window, int32_t * _xpos, int32_t * _ypos) {
    glfwGetWindowPos(_window, _xpos, _ypos);
}
extern "C" void __c__glfwSetWindowPos(GLFWwindow * _window, int32_t _xpos, int32_t _ypos) {
    glfwSetWindowPos(_window, _xpos, _ypos);
}
extern "C" void __c__glfwGetWindowSize(GLFWwindow * _window, int32_t * _width, int32_t * _height) {
    glfwGetWindowSize(_window, _width, _height);
}
extern "C" void __c__glfwSetWindowSizeLimits(GLFWwindow * _window, int32_t _minwidth, int32_t _minheight, int32_t _maxwidth, int32_t _maxheight) {
    glfwSetWindowSizeLimits(_window, _minwidth, _minheight, _maxwidth, _maxheight);
}
extern "C" void __c__glfwSetWindowAspectRatio(GLFWwindow * _window, int32_t _numer, int32_t _denom) {
    glfwSetWindowAspectRatio(_window, _numer, _denom);
}
extern "C" void __c__glfwSetWindowSize(GLFWwindow * _window, int32_t _width, int32_t _height) {
    glfwSetWindowSize(_window, _width, _height);
}
extern "C" void __c__glfwGetFramebufferSize(GLFWwindow * _window, int32_t * _width, int32_t * _height) {
    glfwGetFramebufferSize(_window, _width, _height);
}
extern "C" void __c__glfwGetWindowFrameSize(GLFWwindow * _window, int32_t * _left, int32_t * _top, int32_t * _right, int32_t * _bottom) {
    glfwGetWindowFrameSize(_window, _left, _top, _right, _bottom);
}
extern "C" void __c__glfwGetWindowContentScale(GLFWwindow * _window, float * _xscale, float * _yscale) {
    glfwGetWindowContentScale(_window, _xscale, _yscale);
}
extern "C" void __c__glfwGetWindowOpacity(float *ret, GLFWwindow * _window) {
    *ret = (float )glfwGetWindowOpacity(_window);
}
extern "C" void __c__glfwSetWindowOpacity(GLFWwindow * _window, float _opacity) {
    glfwSetWindowOpacity(_window, _opacity);
}
extern "C" void __c__glfwIconifyWindow(GLFWwindow * _window) {
    glfwIconifyWindow(_window);
}
extern "C" void __c__glfwRestoreWindow(GLFWwindow * _window) {
    glfwRestoreWindow(_window);
}
extern "C" void __c__glfwMaximizeWindow(GLFWwindow * _window) {
    glfwMaximizeWindow(_window);
}
extern "C" void __c__glfwShowWindow(GLFWwindow * _window) {
    glfwShowWindow(_window);
}
extern "C" void __c__glfwHideWindow(GLFWwindow * _window) {
    glfwHideWindow(_window);
}
extern "C" void __c__glfwFocusWindow(GLFWwindow * _window) {
    glfwFocusWindow(_window);
}
extern "C" void __c__glfwRequestWindowAttention(GLFWwindow * _window) {
    glfwRequestWindowAttention(_window);
}
extern "C" void __c__glfwGetWindowMonitor(GLFWmonitor * *ret, GLFWwindow * _window) {
    *ret = (GLFWmonitor * )glfwGetWindowMonitor(_window);
}
extern "C" void __c__glfwSetWindowMonitor(GLFWwindow * _window, GLFWmonitor * _monitor, int32_t _xpos, int32_t _ypos, int32_t _width, int32_t _height, int32_t _refreshRate) {
    glfwSetWindowMonitor(_window, _monitor, _xpos, _ypos, _width, _height, _refreshRate);
}
extern "C" void __c__glfwGetWindowAttrib(int32_t *ret, GLFWwindow * _window, int32_t _attrib) {
    *ret = (int32_t )glfwGetWindowAttrib(_window, _attrib);
}
extern "C" void __c__glfwSetWindowAttrib(GLFWwindow * _window, int32_t _attrib, int32_t _value) {
    glfwSetWindowAttrib(_window, _attrib, _value);
}
extern "C" void __c__glfwSetWindowUserPointer(GLFWwindow * _window, void * _pointer) {
    glfwSetWindowUserPointer(_window, _pointer);
}
extern "C" void __c__glfwGetWindowUserPointer(void * *ret, GLFWwindow * _window) {
    *ret = (void * )glfwGetWindowUserPointer(_window);
}
extern "C" void __c__glfwSetWindowPosCallback(GLFWwindowposfun *ret, GLFWwindow * _window, GLFWwindowposfun _cbfun) {
    *ret = (GLFWwindowposfun )glfwSetWindowPosCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetWindowSizeCallback(GLFWwindowsizefun *ret, GLFWwindow * _window, GLFWwindowsizefun _cbfun) {
    *ret = (GLFWwindowsizefun )glfwSetWindowSizeCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetWindowCloseCallback(GLFWwindowclosefun *ret, GLFWwindow * _window, GLFWwindowclosefun _cbfun) {
    *ret = (GLFWwindowclosefun )glfwSetWindowCloseCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetWindowRefreshCallback(GLFWwindowrefreshfun *ret, GLFWwindow * _window, GLFWwindowrefreshfun _cbfun) {
    *ret = (GLFWwindowrefreshfun )glfwSetWindowRefreshCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetWindowFocusCallback(GLFWwindowfocusfun *ret, GLFWwindow * _window, GLFWwindowfocusfun _cbfun) {
    *ret = (GLFWwindowfocusfun )glfwSetWindowFocusCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetWindowIconifyCallback(GLFWwindowiconifyfun *ret, GLFWwindow * _window, GLFWwindowiconifyfun _cbfun) {
    *ret = (GLFWwindowiconifyfun )glfwSetWindowIconifyCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetWindowMaximizeCallback(GLFWwindowmaximizefun *ret, GLFWwindow * _window, GLFWwindowmaximizefun _cbfun) {
    *ret = (GLFWwindowmaximizefun )glfwSetWindowMaximizeCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetFramebufferSizeCallback(GLFWframebuffersizefun *ret, GLFWwindow * _window, GLFWframebuffersizefun _cbfun) {
    *ret = (GLFWframebuffersizefun )glfwSetFramebufferSizeCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetWindowContentScaleCallback(GLFWwindowcontentscalefun *ret, GLFWwindow * _window, GLFWwindowcontentscalefun _cbfun) {
    *ret = (GLFWwindowcontentscalefun )glfwSetWindowContentScaleCallback(_window, _cbfun);
}
extern "C" void __c__glfwPollEvents() {
    glfwPollEvents();
}
extern "C" void __c__glfwWaitEvents() {
    glfwWaitEvents();
}
extern "C" void __c__glfwWaitEventsTimeout(double _timeout) {
    glfwWaitEventsTimeout(_timeout);
}
extern "C" void __c__glfwPostEmptyEvent() {
    glfwPostEmptyEvent();
}
extern "C" void __c__glfwGetInputMode(int32_t *ret, GLFWwindow * _window, int32_t _mode) {
    *ret = (int32_t )glfwGetInputMode(_window, _mode);
}
extern "C" void __c__glfwSetInputMode(GLFWwindow * _window, int32_t _mode, int32_t _value) {
    glfwSetInputMode(_window, _mode, _value);
}
extern "C" void __c__glfwRawMouseMotionSupported(int32_t *ret) {
    *ret = (int32_t )glfwRawMouseMotionSupported();
}
extern "C" void __c__glfwGetKeyName(char * *ret, int32_t _key, int32_t _scancode) {
    *ret = (char * )glfwGetKeyName(_key, _scancode);
}
extern "C" void __c__glfwGetKeyScancode(int32_t *ret, int32_t _key) {
    *ret = (int32_t )glfwGetKeyScancode(_key);
}
extern "C" void __c__glfwGetKey(int32_t *ret, GLFWwindow * _window, int32_t _key) {
    *ret = (int32_t )glfwGetKey(_window, _key);
}
extern "C" void __c__glfwGetMouseButton(int32_t *ret, GLFWwindow * _window, int32_t _button) {
    *ret = (int32_t )glfwGetMouseButton(_window, _button);
}
extern "C" void __c__glfwGetCursorPos(GLFWwindow * _window, double * _xpos, double * _ypos) {
    glfwGetCursorPos(_window, _xpos, _ypos);
}
extern "C" void __c__glfwSetCursorPos(GLFWwindow * _window, double _xpos, double _ypos) {
    glfwSetCursorPos(_window, _xpos, _ypos);
}
extern "C" void __c__glfwCreateCursor(GLFWcursor * *ret, const GLFWimage * _image, int32_t _xhot, int32_t _yhot) {
    *ret = (GLFWcursor * )glfwCreateCursor(_image, _xhot, _yhot);
}
extern "C" void __c__glfwCreateStandardCursor(GLFWcursor * *ret, int32_t _shape) {
    *ret = (GLFWcursor * )glfwCreateStandardCursor(_shape);
}
extern "C" void __c__glfwDestroyCursor(GLFWcursor * _cursor) {
    glfwDestroyCursor(_cursor);
}
extern "C" void __c__glfwSetCursor(GLFWwindow * _window, GLFWcursor * _cursor) {
    glfwSetCursor(_window, _cursor);
}
extern "C" void __c__glfwSetKeyCallback(GLFWkeyfun *ret, GLFWwindow * _window, GLFWkeyfun _cbfun) {
    *ret = (GLFWkeyfun )glfwSetKeyCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetCharCallback(GLFWcharfun *ret, GLFWwindow * _window, GLFWcharfun _cbfun) {
    *ret = (GLFWcharfun )glfwSetCharCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetCharModsCallback(GLFWcharmodsfun *ret, GLFWwindow * _window, GLFWcharmodsfun _cbfun) {
    *ret = (GLFWcharmodsfun )glfwSetCharModsCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetMouseButtonCallback(GLFWmousebuttonfun *ret, GLFWwindow * _window, GLFWmousebuttonfun _cbfun) {
    *ret = (GLFWmousebuttonfun )glfwSetMouseButtonCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetCursorPosCallback(GLFWcursorposfun *ret, GLFWwindow * _window, GLFWcursorposfun _cbfun) {
    *ret = (GLFWcursorposfun )glfwSetCursorPosCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetCursorEnterCallback(GLFWcursorenterfun *ret, GLFWwindow * _window, GLFWcursorenterfun _cbfun) {
    *ret = (GLFWcursorenterfun )glfwSetCursorEnterCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetScrollCallback(GLFWscrollfun *ret, GLFWwindow * _window, GLFWscrollfun _cbfun) {
    *ret = (GLFWscrollfun )glfwSetScrollCallback(_window, _cbfun);
}
extern "C" void __c__glfwSetDropCallback(GLFWdropfun *ret, GLFWwindow * _window, GLFWdropfun _cbfun) {
    *ret = (GLFWdropfun )glfwSetDropCallback(_window, _cbfun);
}
extern "C" void __c__glfwJoystickPresent(int32_t *ret, int32_t _jid) {
    *ret = (int32_t )glfwJoystickPresent(_jid);
}
extern "C" void __c__glfwGetJoystickAxes(float * *ret, int32_t _jid, int32_t * _count) {
    *ret = (float * )glfwGetJoystickAxes(_jid, _count);
}
extern "C" void __c__glfwGetJoystickButtons(uint8_t * *ret, int32_t _jid, int32_t * _count) {
    *ret = (uint8_t * )glfwGetJoystickButtons(_jid, _count);
}
extern "C" void __c__glfwGetJoystickHats(uint8_t * *ret, int32_t _jid, int32_t * _count) {
    *ret = (uint8_t * )glfwGetJoystickHats(_jid, _count);
}
extern "C" void __c__glfwGetJoystickName(char * *ret, int32_t _jid) {
    *ret = (char * )glfwGetJoystickName(_jid);
}
extern "C" void __c__glfwGetJoystickGUID(char * *ret, int32_t _jid) {
    *ret = (char * )glfwGetJoystickGUID(_jid);
}
extern "C" void __c__glfwSetJoystickUserPointer(int32_t _jid, void * _pointer) {
    glfwSetJoystickUserPointer(_jid, _pointer);
}
extern "C" void __c__glfwGetJoystickUserPointer(void * *ret, int32_t _jid) {
    *ret = (void * )glfwGetJoystickUserPointer(_jid);
}
extern "C" void __c__glfwJoystickIsGamepad(int32_t *ret, int32_t _jid) {
    *ret = (int32_t )glfwJoystickIsGamepad(_jid);
}
extern "C" void __c__glfwSetJoystickCallback(GLFWjoystickfun *ret, GLFWjoystickfun _cbfun) {
    *ret = (GLFWjoystickfun )glfwSetJoystickCallback(_cbfun);
}
extern "C" void __c__glfwUpdateGamepadMappings(int32_t *ret, char * _string) {
    *ret = (int32_t )glfwUpdateGamepadMappings(_string);
}
extern "C" void __c__glfwGetGamepadName(char * *ret, int32_t _jid) {
    *ret = (char * )glfwGetGamepadName(_jid);
}
extern "C" void __c__glfwGetGamepadState(int32_t *ret, int32_t _jid, GLFWgamepadstate * _state) {
    *ret = (int32_t )glfwGetGamepadState(_jid, _state);
}
extern "C" void __c__glfwSetClipboardString(GLFWwindow * _window, char * _string) {
    glfwSetClipboardString(_window, _string);
}
extern "C" void __c__glfwGetClipboardString(char * *ret, GLFWwindow * _window) {
    *ret = (char * )glfwGetClipboardString(_window);
}
extern "C" void __c__glfwGetTime(double *ret) {
    *ret = (double )glfwGetTime();
}
extern "C" void __c__glfwSetTime(double _time) {
    glfwSetTime(_time);
}
extern "C" void __c__glfwGetTimerValue(uint64_t *ret) {
    *ret = (uint64_t )glfwGetTimerValue();
}
extern "C" void __c__glfwGetTimerFrequency(uint64_t *ret) {
    *ret = (uint64_t )glfwGetTimerFrequency();
}
extern "C" void __c__glfwMakeContextCurrent(GLFWwindow * _window) {
    glfwMakeContextCurrent(_window);
}
extern "C" void __c__glfwGetCurrentContext(GLFWwindow * *ret) {
    *ret = (GLFWwindow * )glfwGetCurrentContext();
}
extern "C" void __c__glfwSwapBuffers(GLFWwindow * _window) {
    glfwSwapBuffers(_window);
}
extern "C" void __c__glfwSwapInterval(int32_t _interval) {
    glfwSwapInterval(_interval);
}
extern "C" void __c__glfwExtensionSupported(int32_t *ret, char * _extension) {
    *ret = (int32_t )glfwExtensionSupported(_extension);
}
extern "C" void __c__glfwGetProcAddress(GLFWglproc *ret, char * _procname) {
    *ret = (GLFWglproc )glfwGetProcAddress(_procname);
}
extern "C" void __c__glfwVulkanSupported(int32_t *ret) {
    *ret = (int32_t )glfwVulkanSupported();
}
extern "C" void __c__glfwGetRequiredInstanceExtensions(char * * *ret, uint32_t * _count) {
    *ret = (char * * )glfwGetRequiredInstanceExtensions(_count);
}
