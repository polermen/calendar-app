import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from './components/Auth/Login';
import Register from './components/Auth/Register';
import CalendarView from './components/Calendar/CalendarView';
import { authService } from './services/authService';

function App() {
  const isAuthenticated = authService.isAuthenticated();

  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route
          path="/calendar"
          element={
            isAuthenticated ? (
              <CalendarView />
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />
        <Route
          path="/"
          element={<Navigate to={isAuthenticated ? "/calendar" : "/login"} replace />}
        />
      </Routes>
    </Router>
  );
}

export default App;
