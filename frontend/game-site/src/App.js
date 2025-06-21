import React, { useState, useEffect, useCallback } from 'react';
import './App.css';

const API_BASE = 'https://game-style.onrender.com/api';
const CLOUDINARY_BASE = 'https://res.cloudinary.com/ddkc9yscf/image/upload/';

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

  // Fetch categories
  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const response = await fetch(`${API_BASE}/Games/categories`);
        const data = await response.json();
        setCategories(data);
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

      // Використовуємо передані параметри або стан
      const actualCategory = category !== null ? category : selectedCategory;
      const searchQueryToUse = query !== null ? query : actualSearchQuery;

      switch (currentView) {
        case 'all':
          url = `${API_BASE}/Games?page=${page}`;
          break;
        case 'top-rated':
          url = `${API_BASE}/Games/top-rated?count=20`;
          break;
        case 'recent':
          url = `${API_BASE}/Games/recent?count=20`;
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

      const data = await response.json();

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

  // Контрольований useEffect - спрацьовує тільки при зміні view або category, але не при search
  useEffect(() => {
    if (currentView !== 'search') {
      fetchGames();
    }
  }, [currentView, selectedCategory]); // Видалили fetchGames з залежностей

  // Окремий useEffect для search
  useEffect(() => {
    if (currentView === 'search' && actualSearchQuery) {
      fetchGames(1, null, actualSearchQuery);
    }
  }, [actualSearchQuery]); // Тільки при зміні пошукового запиту

  // Fetch individual game details
  const fetchGameDetails = async (gameId) => {
    try {
      const response = await fetch(`${API_BASE}/Games/${gameId}`);
      const data = await response.json();
      setSelectedGame(data);
    } catch (error) {
      console.error('Error fetching game details:', error);
    }
  };

  const handleSearch = (e) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      console.log('Searching for:', searchQuery);
      
      // Спочатуa змінюємо стан
      setSelectedCategory('');
      setCurrentView('search');
      
      // Потім встановлюємо пошуковий запит, що викличе useEffect для search
      setActualSearchQuery(searchQuery.trim());
      
      // Скидаємо пагінацію
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
    setActualSearchQuery(''); // Очищуємо пошук при виборі категорії
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
            setActualSearchQuery(''); // Очищуємо пошук
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
            setActualSearchQuery(''); // Очищуємо пошук
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
            setActualSearchQuery(''); // Очищуємо пошук
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

      {selectedGame && (
        <div className="modal" onClick={() => setSelectedGame(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <img
                src={`${CLOUDINARY_BASE}${selectedGame.mediaFile?.firstMediaFile || selectedGame.banner}`}
                alt={selectedGame.title}
                className="modal-banner"
              />
              <button
                className="close-btn"
                onClick={() => setSelectedGame(null)}
              >
                ×
              </button>
            </div>
            <div className="modal-body">
              <h2 className="modal-title">{selectedGame.title}</h2>

              <div className="modal-info">
                <div className="info-item">
                  <span className="info-label">Розробник: </span>
                  <span className="info-value">{selectedGame.developer}</span>
                </div>
                <div className="info-item">
                  <span className="info-label">Дата випуску: </span>
                  <span className="info-value">{formatDate(selectedGame.releaseDate)}</span>
                </div>
                <div className="info-item">
                  <span className="info-label">Жанр: </span>
                  <span className="info-value">{selectedGame.category}</span>
                </div>
                <div className="info-item">
                  <span className="info-label">Рейтинг: </span>
                  <span className="info-value">
                    {renderStars(selectedGame.rating)} ({selectedGame.rating}/5)
                  </span>
                </div>
              </div>

              <div className="modal-description">
                <h3>Опис гри:</h3>
                <p>{selectedGame.description}</p>
              </div>

              {selectedGame.systemRequirements && (
                <div className="modal-description">
                  <h3>Системні вимоги:</h3>
                  <p>{selectedGame.systemRequirements}</p>
                </div>
              )}

              {selectedGame.mediaFile && (
                <div className="media-gallery">
                  {selectedGame.mediaFile.icon && (
                    <div className="media-item">
                      <img
                        src={`${CLOUDINARY_BASE}${selectedGame.mediaFile.icon}`}
                        alt="Icon"
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