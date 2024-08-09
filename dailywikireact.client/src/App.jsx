import { useEffect, useState } from 'react';
import './App.css';

function App() {
    const [gameInstance, setGameInstance] = useState();
    
    useEffect(() => {
        populateGameData();
    }, []);

    const contents = gameInstance === undefined
        ? <p><em>Loading... </em></p>
        : <table className="table table-striped" aria-labelledby="tabelLabel">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Category</th>
                    <th>Title</th>
                    <th>Hint 1</th>
                    <th>Hint 2</th>
                </tr>
            </thead>
            <tbody>
                {gameInstance.map(gameInstance =>
                    <tr key={gameInstance.date}>
                        <td>{gameInstance.date}</td>
                        <td>{gameInstance.category}</td>
                        <td>{gameInstance.title}</td>
                        <td>{gameInstance.hint1}</td>
                        <td dangerouslySetInnerHTML={{ __html: gameInstance.hint2 }}></td>
                    </tr>
                )}
            </tbody>
        </table>;

    return (
        <div>
            <h1 id="tabelLabel">Wikipedia daily article guessing game</h1>
            <p>Pick the title of the article from the given options. More hints are given after failed attempts.</p>
            {contents}
        </div>
    );
    
    async function populateGameData() {
        const response = await fetch('gameinstance?category=Physics');
        const data = await response.json();
        setGameInstance(data);
    }
}

export default App;