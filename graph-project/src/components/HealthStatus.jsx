import { useEffect, useState, useCallback, useRef } from "react";
import axios from "axios";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";

export default function HealthStatus() {
  const [stackData, setStackData] = useState([]);
  const [notifData, setNotifData] = useState([]);
  const [summary, setSummary] = useState(null);
  const [isLiveUpdating, setIsLiveUpdating] = useState(false);
  const [lastUpdateTime, setLastUpdateTime] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const [lastUpdated, setLastUpdated] = useState(null);

  const intervalRef = useRef(null);

  const addNotification = useCallback((message, type = "info") => {
    const notification = {
      id: Date.now(),
      message,
      type,
    };
    setNotifications((prev) => [...prev, notification]);

    setTimeout(() => {
      setNotifications((prev) =>
        prev.filter((n) => n.id !== notification.id)
      );
    }, 3000);
  }, []);

  const loadInitialData = useCallback(async () => {
    try {
      const [stackRes, notifRes, summaryRes] = await Promise.all([
        axios.get("/api/health/stackoverflow"),
        axios.get("/api/health/notification"),
        axios.get("/api/health/summary"),
      ]);

      const stackProcessed = stackRes.data.map((item) => ({
        time: new Date(item.CheckedAt).toLocaleTimeString(),
        status: item.Status === "OK" ? 1 : 0,
        checkedAt: new Date(item.CheckedAt),
      }));

      const notifProcessed = notifRes.data.map((item) => ({
        time: new Date(item.CheckedAt).toLocaleTimeString(),
        status: item.Status === "OK" ? 1 : 0,
        checkedAt: new Date(item.CheckedAt),
      }));

      setStackData(stackProcessed);
      setNotifData(notifProcessed);
      setSummary(summaryRes.data);

      const allData = [...stackProcessed, ...notifProcessed];
      if (allData.length > 0) {
        const maxTime = Math.max(...allData.map((d) => d.checkedAt.getTime()));
        setLastUpdateTime(new Date(maxTime));
      }

      setLastUpdated(new Date());
    } catch (error) {
      console.error("Error fetching health data:", error);
      addNotification("Failed to load health data", "error");
    }
  }, [addNotification]);

  const fetchNewData = useCallback(async () => {
    if (!lastUpdateTime) return;

    try {
      const lastCheckTimeParam = lastUpdateTime.toISOString();

      const [stackRes, notifRes] = await Promise.all([
        axios.get(
          `/api/health/stackoverflow/since?lastCheckTime=${encodeURIComponent(
            lastCheckTimeParam
          )}`
        ),
        axios.get(
          `/api/health/notification/since?lastCheckTime=${encodeURIComponent(
            lastCheckTimeParam
          )}`
        ),
      ]);

      const newStackData = stackRes.data.map((item) => ({
        time: new Date(item.CheckedAt).toLocaleTimeString(),
        status: item.Status === "OK" ? 1 : 0,
        checkedAt: new Date(item.CheckedAt),
      }));

      const newNotifData = notifRes.data.map((item) => ({
        time: new Date(item.CheckedAt).toLocaleTimeString(),
        status: item.Status === "OK" ? 1 : 0,
        checkedAt: new Date(item.CheckedAt),
      }));

      if (newStackData.length > 0 || newNotifData.length > 0) {
        setStackData((prev) => [...newStackData, ...prev].slice(0, 50));
        setNotifData((prev) => [...newNotifData, ...prev].slice(0, 50));

        const summaryRes = await axios.get("/api/health/summary");
        setSummary(summaryRes.data);

        const allNewData = [...newStackData, ...newNotifData];
        if (allNewData.length > 0) {
          const maxTime = Math.max(
            ...allNewData.map((d) => d.checkedAt.getTime())
          );
          setLastUpdateTime(new Date(maxTime));
        }

        setLastUpdated(new Date());
        const totalNew = newStackData.length + newNotifData.length;
        addNotification(`${totalNew} new health check(s) received`, "info");
      }
    } catch (error) {
      console.error("Error fetching new data:", error);
      addNotification("Error updating data", "error");
    }
  }, [lastUpdateTime, addNotification]);

  const startLiveUpdating = useCallback(() => {
    if (isLiveUpdating) return;

    setIsLiveUpdating(true);
    addNotification("Live monitoring started", "info");

    intervalRef.current = setInterval(() => {
      fetchNewData();
    }, 2000);
  }, [isLiveUpdating, fetchNewData, addNotification]);

  const stopLiveUpdating = useCallback(() => {
    if (!isLiveUpdating) return;

    setIsLiveUpdating(false);
    addNotification("Live monitoring stopped", "info");

    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
  }, [isLiveUpdating, addNotification]);

  const refreshData = useCallback(async () => {
    await loadInitialData();
    addNotification("Data refreshed", "info");
  }, [loadInitialData, addNotification]);

  useEffect(() => {
    loadInitialData();
  }, [loadInitialData]);

  useEffect(() => {
    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, []);

  const formatTooltip = (value) => {
    return [value === 1 ? "OK" : "NOT_OK", "Status"];
  };

  return (
    <div className="p-6 bg-gradient-to-br from-gray-50 to-gray-100 min-h-screen">
      {/* Notifikacije */}
      <div className="fixed top-4 right-4 z-50 space-y-2">
        {notifications.map((notification) => (
          <div
            key={notification.id}
            className={`px-4 py-2 rounded-xl shadow-lg animate-slide-in-right min-w-48 transition-all duration-300 ${
              notification.type === "error"
                ? "bg-red-500 text-white"
                : "bg-blue-500 text-white"
            }`}
          >
            {notification.message}
          </div>
        ))}
      </div>

      <div className="flex justify-between items-center mb-8">
        <h1 className="text-4xl font-extrabold text-gray-800 drop-shadow-sm">
          Health Monitoring Dashboard
        </h1>
        <div className="flex gap-3">
          <button
            onClick={startLiveUpdating}
            disabled={isLiveUpdating}
            className={`px-5 py-2 rounded-xl font-semibold shadow-md transition-all duration-300 ${
              isLiveUpdating
                ? "bg-gray-300 text-gray-500 cursor-not-allowed"
                : "bg-blue-600 hover:bg-blue-700 text-white"
            }`}
          >
            {isLiveUpdating ? "Monitoring..." : "Start Monitoring"}
          </button>
          <button
            onClick={stopLiveUpdating}
            disabled={!isLiveUpdating}
            className={`px-5 py-2 rounded-xl font-semibold shadow-md transition-all duration-300 ${
              !isLiveUpdating
                ? "bg-gray-300 text-gray-500 cursor-not-allowed"
                : "bg-red-600 hover:bg-red-700 text-white"
            }`}
          >
            Stop Monitoring
          </button>
          <button
            onClick={refreshData}
            className="px-5 py-2 rounded-xl font-semibold shadow-md bg-green-600 hover:bg-green-700 text-white transition-all duration-300"
          >
            🔄 Refresh
          </button>
        </div>
      </div>

      {/* Summary Card */}
      {summary && (
        <div className="bg-white shadow-xl rounded-2xl p-6 mb-8 border border-gray-100">
          <h2 className="text-2xl font-bold mb-4 text-gray-800">
            System Overview
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="text-center p-4 rounded-lg bg-gray-50 shadow-inner">
              <div className="text-2xl font-extrabold mb-2">
                <span
                  className={`${
                    summary.OverallStatus === "OK"
                      ? "text-green-600"
                      : "text-red-600"
                  }`}
                >
                  {summary.OverallStatus}
                </span>
              </div>
              <p className="text-gray-600">Overall Status</p>
            </div>
            <div className="text-center p-4 rounded-lg bg-gray-50 shadow-inner">
              <div className="text-lg font-semibold mb-2">
                <span
                  className={`${
                    summary.StackOverflowService.Status === "OK"
                      ? "text-green-600"
                      : "text-red-600"
                  }`}
                >
                  {summary.StackOverflowService.Status}
                </span>
              </div>
              <p className="text-gray-600">StackOverflow Service</p>
              <p className="text-xs text-gray-500">
                {summary.StackOverflowService.LastChecked
                  ? new Date(
                      summary.StackOverflowService.LastChecked
                    ).toLocaleString()
                  : "Never"}
              </p>
            </div>
            <div className="text-center p-4 rounded-lg bg-gray-50 shadow-inner">
              <div className="text-lg font-semibold mb-2">
                <span
                  className={`${
                    summary.NotificationService.Status === "OK"
                      ? "text-green-600"
                      : "text-red-600"
                  }`}
                >
                  {summary.NotificationService.Status}
                </span>
              </div>
              <p className="text-gray-600">Notification Service</p>
              <p className="text-xs text-gray-500">
                {summary.NotificationService.LastChecked
                  ? new Date(
                      summary.NotificationService.LastChecked
                    ).toLocaleString()
                  : "Never"}
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Charts */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-8 mb-8">
        <div className="bg-white shadow-xl rounded-2xl p-6">
          <h2 className="text-xl font-bold mb-4 text-gray-800">
            StackOverflow Service
          </h2>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={stackData.slice(0, 20).reverse()}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
              <XAxis dataKey="time" tick={{ fontSize: 12 }} stroke="#666" />
              <YAxis
                domain={[0, 1]}
                tickFormatter={(val) => (val === 1 ? "OK" : "NOT_OK")}
                tick={{ fontSize: 12 }}
                stroke="#666"
              />
              <Tooltip formatter={formatTooltip} />
              <Legend />
              <Line
                type="stepAfter"
                dataKey="status"
                stroke="#10b981"
                strokeWidth={3}
                dot={{ r: 4, fill: "#10b981" }}
                activeDot={{ r: 6, fill: "#10b981" }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white shadow-xl rounded-2xl p-6">
          <h2 className="text-xl font-bold mb-4 text-gray-800">
            Notification Service
          </h2>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={notifData.slice(0, 20).reverse()}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
              <XAxis dataKey="time" tick={{ fontSize: 12 }} stroke="#666" />
              <YAxis
                domain={[0, 1]}
                tickFormatter={(val) => (val === 1 ? "OK" : "NOT_OK")}
                tick={{ fontSize: 12 }}
                stroke="#666"
              />
              <Tooltip formatter={formatTooltip} />
              <Legend />
              <Line
                type="stepAfter"
                dataKey="status"
                stroke="#ef4444"
                strokeWidth={3}
                dot={{ r: 4, fill: "#ef4444" }}
                activeDot={{ r: 6, fill: "#ef4444" }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Footer */}
      <div className="text-center text-gray-500 text-sm">
        {lastUpdated && `Last updated: ${lastUpdated.toLocaleString()}`}
        {isLiveUpdating && (
          <span className="ml-3 px-3 py-1 bg-green-200 text-green-800 rounded-full text-xs font-semibold">
            ● Live Monitoring Active
          </span>
        )}
      </div>
    </div>
  );
}
