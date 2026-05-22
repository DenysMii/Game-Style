import React, { useState, useEffect, useCallback } from 'react';
import './App.css';

const API_BASE = process.env.REACT_APP_API_BASE || 'https://game-style.onrender.com/api';
const CLOUDINARY_BASE = 'https://res.cloudinary.com/ddkc9yscf/image/upload/';

const parseJsonResponse = async (response) => {
  const responseText = await response.text();

  if (!responseText) {
    return {};
  }

  try {
    return JSON.parse(responseText);
  } catch {
    return {};
  }
};

function App() {
  const [games, setGames] = useState([]);
  const [categories, setCategories] = useState([]);
  const [selectedGame, setSelectedGame] = useState(null);
  const [currentView, setCurrentView] = useState('all');
  const [selectedCategory, setSelectedCategory] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [actualSearchQuery, setActualSearchQuery] = useState('');
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({
    page: 1,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false
  });
  const [authMode, setAuthMode] = useState(null);
  const [authForm, setAuthForm] = useState({
    username: '',
    email: '',
    password: '',
    login: ''
  });
  const [authMessage, setAuthMessage] = useState('');
  const [authLoading, setAuthLoading] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const [uploadModalOpen, setUploadModalOpen] = useState(false);
  const [uploadLoading, setUploadLoading] = useState(false);
  const [uploadMessage, setUploadMessage] = useState('');
  const [uploadForm, setUploadForm] = useState({
    title: '',
    description: '',
    category: '',
    systemRequirements: '',
    icon: null,
    gameplayImages: [],
    archive: null
  });

  // Fetch categories
  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const response = await fetch(`${API_BASE}/Games/categories`);
        const data = await parseJsonResponse(response);
        setCategories(Array.isArray(data) ? data : []);
      } catch (error) {
        console.error('Error fetching categories:', error);
      }
    };
    fetchCategories();
  }, []);

  // Fetch games based on current view
  const fetchGames = useCallback(async (page = 1, category = null, query = null) => {
    setLoading(true);

    try {
      let url = `${API_BASE}/Games`;

      // Use passed parameters or current state
      const actualCategory = category !== null ? category : selectedCategory;
      const searchQueryToUse = query !== null ? query : actualSearchQuery;

      switch (currentView) {
        case 'all':
          url = `${API_BASE}/Games?page=${page}`;
          break;
        case 'top-rated':
          url = `${API_BASE}/Games/top-rated?count=12`;
          break;
        case 'recent':
          url = `${API_BASE}/Games/recent?count=12`;
          break;
        case 'category':
          url = `${API_BASE}/Games/category/${encodeURIComponent(actualCategory)}?page=${page}`;
          break;
        case 'search':
          url = `${API_BASE}/Games/search?query=${encodeURIComponent(searchQueryToUse)}&page=${page}`;
          break;
        default:
          url = `${API_BASE}/Games?page=${page}`;
      }

      console.log('Fetching URL:', url);

      const response = await fetch(url);

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await parseJsonResponse(response);

      if (data.items) {
        setGames(data.items);
        setPagination({
          page: data.page,
          totalPages: data.totalPages,
          hasNextPage: data.hasNextPage,
          hasPreviousPage: data.hasPreviousPage
        });
      } else {
        setGames(Array.isArray(data) ? data : []);
        setPagination({
          page: 1,
          totalPages: 1,
          hasNextPage: false,
          hasPreviousPage: false
        });
      }
    } catch (error) {
      console.error('Error fetching games:', error);
      setGames([]);
      setPagination({
        page: 1,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false
      });
    } finally {
      setLoading(false);
    }
  }, [currentView, selectedCategory, actualSearchQuery]);

  // Controlled useEffect - runs only when view or category changes, but not for search
  useEffect(() => {
    if (currentView !== 'search') {
      fetchGames();
    }
  }, [currentView, selectedCategory]); // Removed fetchGames from dependencies

  // Separate useEffect for search
  useEffect(() => {
    if (currentView === 'search' && actualSearchQuery) {
      fetchGames(1, null, actualSearchQuery);
    }
  }, [actualSearchQuery]); // Only when the search query changes

  // Fetch individual game details
  const fetchGameDetails = async (gameId) => {
    try {
      const response = await fetch(`${API_BASE}/Games/${gameId}`);
      const data = await parseJsonResponse(response);

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      setSelectedGame(data);
    } catch (error) {
      console.error('Error fetching game details:', error);
    }
  };

  const handleSearch = (e) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      console.log('Searching for:', searchQuery);

      // First update state
      setSelectedCategory('');
      setCurrentView('search');

      // Then set the search query, which triggers the search useEffect
      setActualSearchQuery(searchQuery.trim());

      // Reset pagination
      setPagination({
        page: 1,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false
      });
    }
  };

  const handleCategorySelect = (category) => {
    console.log('Selected category:', category);

    if (selectedCategory === category && currentView === 'category') {
      return;
    }

    setSelectedCategory(category);
    setCurrentView('category');
    setActualSearchQuery(''); // Clear search when selecting a category
    setPagination({
      page: 1,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false
    });
  };

  const handleDownload = (downloadLink) => {
    const url = `https://drive.google.com/uc?export=download&id=${downloadLink}`;
    window.open(url, '_blank');
  };

  const openAuthModal = (mode) => {
    setAuthMode(mode);
    setAuthMessage('');
    setAuthForm({
      username: '',
      email: '',
      password: '',
      login: ''
    });
  };

  const handleAuthInputChange = (e) => {
    const { name, value } = e.target;
    setAuthForm((previousForm) => ({
      ...previousForm,
      [name]: value
    }));
  };

  const handleAuthSubmit = async (e) => {
    e.preventDefault();
    setAuthLoading(true);
    setAuthMessage('');

    const isRegister = authMode === 'register';
    const url = `${API_BASE}/Auth/${isRegister ? 'register' : 'login'}`;
    const body = isRegister
      ? {
          username: authForm.username,
          email: authForm.email,
          password: authForm.password
        }
      : {
          login: authForm.login,
          password: authForm.password
        };

    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(body)
      });
      const data = await parseJsonResponse(response);

      if (!response.ok) {
        throw new Error(data.message || `Помилка авторизації (${response.status})`);
      }

      setCurrentUser(data);
      setAuthMode(null);
    } catch (error) {
      setAuthMessage(error.message);
    } finally {
      setAuthLoading(false);
    }
  };

  const openUploadModal = () => {
    setUploadMessage('');
    setUploadForm({
      title: '',
      description: '',
      category: categories[0] || '',
      systemRequirements: '',
      icon: null,
      gameplayImages: [],
      archive: null
    });
    setUploadModalOpen(true);
  };

  const handleUploadInputChange = (e) => {
    const { name, value, files } = e.target;

    if (name === 'icon') {
      setUploadForm((previousForm) => ({
        ...previousForm,
        icon: files[0] || null
      }));
      return;
    }

    if (name === 'gameplayImages') {
      setUploadForm((previousForm) => ({
        ...previousForm,
        gameplayImages: Array.from(files).slice(0, 4)
      }));
      return;
    }

    if (name === 'archive') {
      setUploadForm((previousForm) => ({
        ...previousForm,
        archive: files[0] || null
      }));
      return;
    }

    setUploadForm((previousForm) => ({
      ...previousForm,
      [name]: value
    }));
  };

  const handleUploadSubmit = async (e) => {
    e.preventDefault();

    if (!currentUser) {
      setUploadMessage('Увійдіть, щоб завантажити гру.');
      return;
    }

    setUploadLoading(true);
    setUploadMessage('');

    const formData = new FormData();
    formData.append('userId', currentUser.id);
    formData.append('username', currentUser.username);
    formData.append('title', uploadForm.title);
    formData.append('description', uploadForm.description);
    formData.append('category', uploadForm.category);
    formData.append('systemRequirements', uploadForm.systemRequirements);
    formData.append('icon', uploadForm.icon);
    formData.append('archive', uploadForm.archive);
    uploadForm.gameplayImages.forEach((file) => {
      formData.append('gameplayImages', file);
    });

    try {
      const response = await fetch(`${API_BASE}/MemberGames/upload`, {
        method: 'POST',
        body: formData
      });
      const data = await parseJsonResponse(response);

      if (!response.ok) {
        const errorMessage = data.message || data.detail || data.title || `Помилка завантаження (${response.status})`;
        throw new Error(errorMessage);
      }

      setUploadModalOpen(false);
      setCurrentView('recent');
      setSelectedCategory('');
      setActualSearchQuery('');
      fetchGames();
    } catch (error) {
      setUploadMessage(error.message);
    } finally {
      setUploadLoading(false);
    }
  };

  const renderStars = (rating) => {
    const stars = [];
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 !== 0;

    for (let i = 0; i < fullStars; i++) {
      stars.push('★');
    }
    if (hasHalfStar) {
      stars.push('☆');
    }

    return stars.join('');
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('uk-UA');
  };

  return (
    <div className="container">
      <header className="header">
        <div className="auth-panel">
          {currentUser ? (
            <div className="member-actions">
              <span className="auth-user">Вітаємо, {currentUser.username}</span>
              <button className="auth-open-btn" onClick={openUploadModal}>
                Додати гру
              </button>
            </div>
          ) : (
            <button className="auth-open-btn" onClick={() => openAuthModal('login')}>
              Увійти
            </button>
          )}
        </div>
        <h1 className="logo">GAME STYLE</h1>
        <p className="tagline">Твоє джерело кращих ігор</p>
        <form className="search-container" onSubmit={handleSearch}>
          <input
            type="text"
            className="search-input"
            placeholder="Шукати ігри..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
          <button type="submit" className="search-btn" aria-label="Пошук">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              height="20"
              width="20"
              fill="white"
              viewBox="0 0 24 24"
            >
              <path d="M21 20l-5.6-5.6A7.9 7.9 0 0018 10a8 8 0 10-8 8 7.9 7.9 0 004.4-1.4L20 21zM4 10a6 6 0 1112 0 6 6 0 01-12 0z" />
            </svg>
          </button>
        </form>
      </header>

      <nav className="navigation">
        <button
          className={`nav-btn ${currentView === 'all' && selectedCategory === '' ? 'active' : ''}`}
          onClick={() => {
            if (currentView === 'all' && selectedCategory === '') return;
            setSelectedCategory('');
            setActualSearchQuery(''); // Clear search
            setCurrentView('all');
          }}
        >
          Всі ігри
        </button>
        <button
          className={`nav-btn ${currentView === 'top-rated' ? 'active' : ''}`}
          onClick={() => {
            if (currentView === 'top-rated') return;
            setSelectedCategory('');
            setActualSearchQuery(''); // Clear search
            setCurrentView('top-rated');
          }}
        >
          Топ рейтинг
        </button>
        <button
          className={`nav-btn ${currentView === 'recent' ? 'active' : ''}`}
          onClick={() => {
            if (currentView === 'recent') return;
            setSelectedCategory('');
            setActualSearchQuery(''); // Clear search
            setCurrentView('recent');
          }}
        >
          Останні
        </button>
      </nav>

      <div className="categories">
        {categories.map((category) => (
          <button
            key={category}
            className={`category-btn ${selectedCategory === category && currentView === 'category' ? 'active' : ''}`}
            onClick={() => handleCategorySelect(category)}
          >
            {category}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="loading">
          <div className="spinner"></div>
          Завантаження...
        </div>
      ) : (
        <>
          <div className="games-grid">
            {games.map((game) => (
              <div
                key={game.id}
                className="game-card"
                onClick={() => fetchGameDetails(game.id)}
              >
                <img
                  src={`${CLOUDINARY_BASE}${game.banner}`}
                  alt={game.title}
                  className="game-banner"
                  onError={(e) => {
                    e.target.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjgwIiBoZWlnaHQ9IjIwMCIgdmlld0JveD0iMCAwIDI4MCAyMDAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIyODAiIGhlaWdodD0iMjAwIiBmaWxsPSIjRjNGNEY2Ii8+CjxwYXRoIGQ9Ik0xNDAgMTAwTDEyMCA4MEwxNjAgODBMMTQwIDEwMFoiIGZpbGw9IiNEMUQ1REIiLz4KPC9zdmc+';
                  }}
                />
                <div className="game-content">
                  <h3 className="game-title">{game.title}</h3>
                  <p className="game-description">{game.description}</p>
                </div>
              </div>
            ))}
          </div>

          {pagination.totalPages > 1 && (
            <div className="pagination">
              {pagination.hasPreviousPage && (
                <button
                  className="page-btn"
                  onClick={() => fetchGames(pagination.page - 1)}
                >
                  ← Попередня
                </button>
              )}
              <span className="page-btn active">
                {pagination.page} з {pagination.totalPages}
              </span>
              {pagination.hasNextPage && (
                <button
                  className="page-btn"
                  onClick={() => fetchGames(pagination.page + 1)}
                >
                  Наступна →
                </button>
              )}
            </div>
          )}
        </>
      )}

      {authMode && (
        <div className="modal" onClick={() => setAuthMode(null)}>
          <div className="modal-content auth-modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="auth-modal-header">
              <h2 className="auth-title">
                {authMode === 'register' ? 'Реєстрація' : 'Вхід'}
              </h2>
              <button
                className="close-btn"
                onClick={() => setAuthMode(null)}
              >
                ×
              </button>
            </div>
            <form className="auth-form" onSubmit={handleAuthSubmit}>
              {authMode === 'register' ? (
                <>
                  <input
                    type="text"
                    name="username"
                    className="auth-input"
                    placeholder="Ім'я користувача"
                    value={authForm.username}
                    onChange={handleAuthInputChange}
                    required
                  />
                  <input
                    type="email"
                    name="email"
                    className="auth-input"
                    placeholder="Email"
                    value={authForm.email}
                    onChange={handleAuthInputChange}
                    required
                  />
                </>
              ) : (
                <input
                  type="text"
                  name="login"
                  className="auth-input"
                  placeholder="Ім'я користувача/Email"
                  value={authForm.login}
                  onChange={handleAuthInputChange}
                  required
                />
              )}
              <input
                type="password"
                name="password"
                className="auth-input"
                placeholder="Пароль"
                value={authForm.password}
                onChange={handleAuthInputChange}
                required
              />
              {authMessage && <p className="auth-message">{authMessage}</p>}
              <button type="submit" className="download-btn" disabled={authLoading}>
                {authLoading ? 'Зачекайте...' : authMode === 'register' ? 'Зареєструватися' : 'Увійти'}
              </button>
              <button
                type="button"
                className="auth-switch"
                onClick={() => openAuthModal(authMode === 'register' ? 'login' : 'register')}
              >
                {authMode === 'register'
                  ? 'Вже маєте акаунт? Увійти'
                  : 'Немає акаунта? Зареєструватися'}
              </button>
            </form>
          </div>
        </div>
      )}

      {uploadModalOpen && (
        <div className="modal" onClick={() => setUploadModalOpen(false)}>
          <div className="modal-content upload-modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="auth-modal-header">
              <h2 className="auth-title">Завантажити гру</h2>
              <button
                className="close-btn"
                onClick={() => setUploadModalOpen(false)}
              >
                ×
              </button>
            </div>
            <form className="auth-form upload-form" onSubmit={handleUploadSubmit}>
              <input
                type="text"
                name="title"
                className="auth-input"
                placeholder="Назва"
                value={uploadForm.title}
                onChange={handleUploadInputChange}
                required
              />
              <textarea
                name="description"
                className="auth-input upload-textarea"
                placeholder="Опис гри"
                value={uploadForm.description}
                onChange={handleUploadInputChange}
                required
              />
              <textarea
                name="systemRequirements"
                className="auth-input upload-textarea"
                placeholder="Системні вимоги"
                value={uploadForm.systemRequirements}
                onChange={handleUploadInputChange}
                required
              />
              <select
                name="category"
                className="auth-input"
                value={uploadForm.category}
                onChange={handleUploadInputChange}
                required
              >
                <option value="" disabled>Жанр</option>
                {categories.map((category) => (
                  <option key={category} value={category}>
                    {category}
                  </option>
                ))}
              </select>
              <label className="upload-label">
                Іконка гри
                <input
                  type="file"
                  name="icon"
                  accept="image/*"
                  onChange={handleUploadInputChange}
                  required
                />
              </label>
              <label className="upload-label">
                Зображення геймплею
                <input
                  type="file"
                  name="gameplayImages"
                  accept="image/*"
                  multiple
                  onChange={handleUploadInputChange}
                  required
                />
              </label>
              <label className="upload-label">
                Архів гри
                <input
                  type="file"
                  name="archive"
                  accept=".zip,.rar,.7z,.tar,.gz,.bz2,application/zip,application/x-rar-compressed,application/x-7z-compressed"
                  onChange={handleUploadInputChange}
                  required
                />
              </label>
              {uploadMessage && <p className="auth-message">{uploadMessage}</p>}
              <button type="submit" className="download-btn" disabled={uploadLoading}>
                {uploadLoading ? 'Завантаження...' : 'Опублікувати гру'}
              </button>
            </form>
          </div>
        </div>
      )}

      {selectedGame && (
        <div className="modal" onClick={() => setSelectedGame(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              {selectedGame.mediaFile.icon && (
                <div className="modal-icon">
                  <img
                    src={`${CLOUDINARY_BASE}${selectedGame.mediaFile.icon}`}
                    alt="Icon"
                    className="modal-icon"
                  />
                </div>
              )}
              <div>
                <h1 className="modal-title">{selectedGame.title}</h1>
                <div className="modal-rating">
                  <span className="stars">{renderStars(selectedGame.rating)}</span>
                  <span>({selectedGame.rating}/5)</span>
                </div>
              </div>
              <button
                className="close-btn"
                onClick={() => setSelectedGame(null)}
              >
                ×
              </button>
            </div>
            <div className="modal-body">
              <div className="modal-info">
                <span className="modal-info-strong">Розробник: </span>
                <span>{selectedGame.developer}</span>
              </div>
              <div className="modal-info">
                <span className="modal-info-strong">Дата випуску: </span>
                <span>{formatDate(selectedGame.releaseDate)}</span>
              </div>
              <div className="modal-info">
                <span className="modal-info-strong">Жанр: </span>
                <span >{selectedGame.category}</span>
              </div>

              {selectedGame.description && (
                <div className="modal-info">
                  <h3>Опис гри:</h3>
                  <p>{selectedGame.description}</p>
                </div>
              )}

              {selectedGame.systemRequirements && (
                <div className="modal-info">
                  <h3>Системні вимоги:</h3>
                  {selectedGame.systemRequirements.split('\n').map((line, index) => (
                    <p key={index}>{line}</p>
                  ))}
                </div>
              )}

              {selectedGame.mediaFile && (
                <div className="media-gallery">
                  {selectedGame.mediaFile.firstMediaFile && (
                    <div className="media-item">
                      <img
                        src={`${CLOUDINARY_BASE}${selectedGame.mediaFile.firstMediaFile}`}
                        alt="Screenshot 1"
                        className="media-image"
                      />
                    </div>
                  )}
                  {selectedGame.mediaFile.secondMediaFile && (
                    <div className="media-item">
                      <img
                        src={`${CLOUDINARY_BASE}${selectedGame.mediaFile.secondMediaFile}`}
                        alt="Screenshot 2"
                        className="media-image"
                      />
                    </div>
                  )}
                  {selectedGame.mediaFile.thirdMediaFile && (
                    <div className="media-item">
                      <img
                        src={`${CLOUDINARY_BASE}${selectedGame.mediaFile.thirdMediaFile}`}
                        alt="Screenshot 3"
                        className="media-image"
                      />
                    </div>
                  )}
                  {selectedGame.mediaFile.fourthMediaFile && (
                    <div className="media-item">
                      <img
                        src={`${CLOUDINARY_BASE}${selectedGame.mediaFile.fourthMediaFile}`}
                        alt="Screenshot 4"
                        className="media-image"
                      />
                    </div>
                  )}
                </div>
              )}

              {selectedGame.downloadLink && (
                <button
                  className="download-btn"
                  onClick={() => handleDownload(selectedGame.downloadLink)}
                >
                  Завантажити гру
                </button>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
