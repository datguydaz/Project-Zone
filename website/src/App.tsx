import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { Layout } from "./components/Layout";
import { HubProvider } from "./lib/HubContext";
import { DashboardPage } from "./pages/DashboardPage";
import { DevLogPage } from "./pages/DevLogPage";
import { RoadmapPage } from "./pages/RoadmapPage";
import { TasksPage } from "./pages/TasksPage";

const basename = import.meta.env.BASE_URL.replace(/\/$/, "") || undefined;

export default function App() {
  return (
    <HubProvider>
      <BrowserRouter basename={basename}>
        <Routes>
          <Route element={<Layout />}>
            <Route index element={<DashboardPage />} />
            <Route path="roadmap" element={<RoadmapPage />} />
            <Route path="tasks" element={<TasksPage />} />
            <Route path="devlog" element={<DevLogPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </HubProvider>
  );
}
