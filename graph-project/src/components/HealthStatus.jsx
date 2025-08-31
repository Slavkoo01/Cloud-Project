import { useEffect, useState, useCallback, useRef } from "react";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import { Activity, Server, Bell, RefreshCw, Play, Square } from "lucide-react";

export default function HealthStatus() {
  const [stackData, setStackData] = useState([]);
  const [notifData, setNotifData] = useState([]);
  const [summary, setSummary] = useState(null);
  const [isLiveUpdating, setIsLiveUpdating] = useState(false);
  const [lastUpdateTime, setLastUpdateTime] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const [lastUpdated, setLastUpdated] = useState(null);
  
  const intervalRef = useRef(null);

  // Mock data za demo
  useEffect(() => {
    const generateMockData = () => {
      const now = new Date();
      const data = [];
      for (let i = 19; i >= 0; i--) {
        const time = new Date(now.getTime() - i * 30000);
        data.push({
          time: time.toLocaleTimeString(),
          status: Math.random() > 0.1 ? 1 : 0,
          checkedAt: time
        });
      }
      return data;
    };

    setStackData(generateMockData());
    setNotifData(generateMockData());
    setSummary({
      OverallStatus: 'OK',
      StackOverflowService: {
        Status: 'OK',
        LastChecked: new Date().toISOString()
      },
      NotificationService: {
        Status: 'OK',
        LastChecked: new Date().toISOString()
      }
    });
    setLastUpdated(new Date());
  }, []);

  // Funkcija za dodavanje notifikacije
  const addNotification = useCallback((message, type = 'info') => {
    const notification = {
      id: Date.now(),
      message,
      type
    };
    setNotifications(prev => [...prev, notification]);
    
    setTimeout(() => {
      setNotifications(prev => prev.filter(n => n.id !== notification.id));
    }, 3000);
  }, []);

  // MOCK FUNKCIJE ZA DEMO JER MI NE VUCE SA BACK-A KAKO TREBA
  const startLiveUpdating = useCallback(() => {
    if (isLiveUpdating) return;
    setIsLiveUpdating(true);
    addNotification('Live monitoring started', 'success');
  }, [isLiveUpdating, addNotification]);

  const stopLiveUpdating = useCallback(() => {
    if (!isLiveUpdating) return;
    setIsLiveUpdating(false);
    addNotification('Live monitoring stopped', 'info');
  }, [isLiveUpdating, addNotification]);

  const refreshData = useCallback(() => {
    addNotification('Data refreshed', 'success');
    setLastUpdated(new Date());
  }, [addNotification]);

  const formatTooltip = (value, name) => {
    return [value === 1 ? 'Healthy' : 'Down', 'Status'];
  };

  const calculateUptime = (data) => {
    if (!data || data.length === 0) return 100;
    const okCount = data.filter(d => d.status === 1).length;
    return ((okCount / data.length) * 100).toFixed(1);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Notifications */}
      <div className="fixed top-4 right-4 z-50 space-y-2">
        {notifications.map(notification => (
          <div
            key={notification.id}
            className={`px-4 py-2 rounded-lg shadow-lg border transition-all duration-300 ${
              notification.type === 'error' 
                ? 'bg-red-50 text-red-700 border-red-200' 
                : notification.type === 'success'
                ? 'bg-green-50 text-green-700 border-green-200'
                : 'bg-blue-50 text-blue-700 border-blue-200'
            }`}
          >
            {notification.message}
          </div>
        ))}
      </div>

      <div className="p-6 max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-blue-600 rounded-lg flex items-center justify-center">
              <Activity className="w-5 h-5 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Health Monitor</h1>
              <p className="text-gray-600 text-sm">System status dashboard</p>
            </div>
          </div>
          
          <div className="flex gap-2">
            <button
              onClick={startLiveUpdating}
              disabled={isLiveUpdating}
              className={`px-4 py-2 rounded-lg font-medium flex items-center gap-2 transition-colors ${
                isLiveUpdating
                  ? 'bg-gray-200 text-gray-500 cursor-not-allowed'
                  : 'bg-green-600 text-white hover:bg-green-700'
              }`}
            >
              <Play className="w-4 h-4" />
              {isLiveUpdating ? 'Monitoring' : 'Start'}
            </button>
            
            <button
              onClick={stopLiveUpdating}
              disabled={!isLiveUpdating}
              className={`px-4 py-2 rounded-lg font-medium flex items-center gap-2 transition-colors ${
                !isLiveUpdating
                  ? 'bg-gray-200 text-gray-500 cursor-not-allowed'
                  : 'bg-red-600 text-white hover:bg-red-700'
              }`}
            >
              <Square className="w-4 h-4" />
              Stop
            </button>
            
            <button
              onClick={refreshData}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 flex items-center gap-2 transition-colors"
            >
              <RefreshCw className="w-4 h-4" />
              Refresh
            </button>
          </div>
        </div>

        {/* Status Cards */}
        {summary && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
            {/* Overall Status */}
            <div className="bg-white rounded-lg p-6 shadow-sm border">
              <div className="flex items-center justify-between mb-4">
                <Activity className={`w-6 h-6 ${
                  summary.OverallStatus === 'OK' ? 'text-green-600' : 'text-red-600'
                }`} />
                {isLiveUpdating && (
                  <div className="flex items-center gap-1">
                    <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
                    <span className="text-xs text-green-600 font-medium">LIVE</span>
                  </div>
                )}
              </div>
              <div className={`text-2xl font-bold mb-1 ${
                summary.OverallStatus === 'OK' ? 'text-green-600' : 'text-red-600'
              }`}>
                {summary.OverallStatus}
              </div>
              <p className="text-gray-600 font-medium">Overall Status</p>
            </div>

            {/* StackOverflow Service */}
            <div className="bg-white rounded-lg p-6 shadow-sm border">
              <div className="flex items-center justify-between mb-4">
                <Server className={`w-6 h-6 ${
                  summary.StackOverflowService.Status === 'OK' ? 'text-green-600' : 'text-red-600'
                }`} />
                <div className="text-right">
                  <div className="text-xs text-gray-500">UPTIME</div>
                  <div className="text-sm font-bold text-gray-900">{calculateUptime(stackData)}%</div>
                </div>
              </div>
              <div className={`text-xl font-bold mb-1 ${
                summary.StackOverflowService.Status === 'OK' ? 'text-green-600' : 'text-red-600'
              }`}>
                {summary.StackOverflowService.Status}
              </div>
              <p className="text-gray-600 font-medium">StackOverflow</p>
              <p className="text-xs text-gray-500 mt-2">
                {summary.StackOverflowService.LastChecked 
                  ? new Date(summary.StackOverflowService.LastChecked).toLocaleTimeString() 
                  : 'Never'
                }
              </p>
            </div>

            {/* Notification Service */}
            <div className="bg-white rounded-lg p-6 shadow-sm border">
              <div className="flex items-center justify-between mb-4">
                <Bell className={`w-6 h-6 ${
                  summary.NotificationService.Status === 'OK' ? 'text-green-600' : 'text-red-600'
                }`} />
                <div className="text-right">
                  <div className="text-xs text-gray-500">UPTIME</div>
                  <div className="text-sm font-bold text-gray-900">{calculateUptime(notifData)}%</div>
                </div>
              </div>
              <div className={`text-xl font-bold mb-1 ${
                summary.NotificationService.Status === 'OK' ? 'text-green-600' : 'text-red-600'
              }`}>
                {summary.NotificationService.Status}
              </div>
              <p className="text-gray-600 font-medium">Notifications</p>
              <p className="text-xs text-gray-500 mt-2">
                {summary.NotificationService.LastChecked 
                  ? new Date(summary.NotificationService.LastChecked).toLocaleTimeString() 
                  : 'Never'
                }
              </p>
            </div>
          </div>
        )}

        {/* Charts */}
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6 mb-6">
          {/* StackOverflow Service Chart */}
          <div className="bg-white rounded-lg p-6 shadow-sm border">
            <div className="flex items-center gap-3 mb-4">
              <Server className="w-5 h-5 text-blue-600" />
              <div>
                <h3 className="font-semibold text-gray-900">StackOverflow Service</h3>
                <p className="text-sm text-gray-500">Last 20 checks</p>
              </div>
            </div>
            
            <ResponsiveContainer width="100%" height={250}>
              <LineChart data={stackData.slice(0, 20).reverse()}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                <XAxis 
                  dataKey="time" 
                  tick={{ fontSize: 12 }}
                  stroke="#9ca3af"
                />
                <YAxis
                  domain={[0, 1]}
                  tickFormatter={(val) => (val === 1 ? "OK" : "DOWN")}
                  tick={{ fontSize: 12 }}
                  stroke="#9ca3af"
                />
                <Tooltip formatter={formatTooltip} />
                <Line
                  type="stepAfter"
                  dataKey="status"
                  stroke="#10b981"
                  strokeWidth={2}
                  dot={{ r: 3, fill: '#10b981' }}
                  activeDot={{ r: 5, fill: '#10b981' }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>

          {/* Notification Service Chart */}
          <div className="bg-white rounded-lg p-6 shadow-sm border">
            <div className="flex items-center gap-3 mb-4">
              <Bell className="w-5 h-5 text-blue-600" />
              <div>
                <h3 className="font-semibold text-gray-900">Notification Service</h3>
                <p className="text-sm text-gray-500">Last 20 checks</p>
              </div>
            </div>
            
            <ResponsiveContainer width="100%" height={250}>
              <LineChart data={notifData.slice(0, 20).reverse()}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                <XAxis 
                  dataKey="time" 
                  tick={{ fontSize: 12 }}
                  stroke="#9ca3af"
                />
                <YAxis
                  domain={[0, 1]}
                  tickFormatter={(val) => (val === 1 ? "OK" : "DOWN")}
                  tick={{ fontSize: 12 }}
                  stroke="#9ca3af"
                />
                <Tooltip formatter={formatTooltip} />
                <Line
                  type="stepAfter"
                  dataKey="status"
                  stroke="#3b82f6"
                  strokeWidth={2}
                  dot={{ r: 3, fill: '#3b82f6' }}
                  activeDot={{ r: 5, fill: '#3b82f6' }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Footer */}
        <div className="flex flex-col sm:flex-row justify-between items-center gap-4 p-4 bg-white rounded-lg shadow-sm border">
          <div className="flex items-center gap-2 text-gray-600 text-sm">
            <RefreshCw className="w-4 h-4" />
            {lastUpdated && `Last updated: ${lastUpdated.toLocaleString()}`}
          </div>
          
          {isLiveUpdating ? (
            <div className="flex items-center gap-2 px-3 py-1 bg-green-100 text-green-800 rounded-full text-sm">
              <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
              Live Monitoring
            </div>
          ) : (
            <div className="flex items-center gap-2 px-3 py-1 bg-gray-100 text-gray-600 rounded-full text-sm">
              <div className="w-2 h-2 bg-gray-400 rounded-full"></div>
              Monitoring Stopped
            </div>
          )}
        </div>
      </div>
    </div>
  );
}