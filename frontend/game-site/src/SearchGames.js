import React, { useState, useEffect } from "react";

function SearchGames() {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState([]);

  useEffect(() => {
    if (!query) {
      setResults([]);
      return;
    }
    const timer = setTimeout(() => {
      fetch(`/api/games/search?query=${encodeURIComponent(query)}`)
        .then(res => res.json())
        .then(data => setResults(data))
        .catch(() => setResults([]));
    }, 500);

    return () => clearTimeout(timer);
  }, [query]);

  return (
    <div>
      <input
        placeholder="Search games..."
        value={query}
        onChange={e => setQuery(e.target.value)}
      />
      <ul>
        {results.map(game => (
          <li key={game.id}>{game.title}</li>
        ))}
      </ul>
    </div>
  );
}

export default SearchGames;
