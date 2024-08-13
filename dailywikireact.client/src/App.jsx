import { useState } from 'react';
import './App.css';

const categories = ["Physics", "Countries in Europe", "States of the United States", "Chemical elements", "Fish common names", "Chinese inventions"];

function App() {
    const [gameInstance, setGameInstance] = useState();

    var contents = <p><em>Loading... </em></p>;
    if (gameInstance === undefined) {
        const gameStartingDiv =
            <div>
                {categories.map(category =>
                    <button key={category} onClick={async () => {
                        populateGameData(category);
                    }}>{category}</button>
                )}
            </div>;
        contents = gameStartingDiv;

    }
    else {
        if (!gameInstance[0].gameEnd) {
            const gameActiveDiv = <div>
                <p>Date: {gameInstance[0].date}</p>
                <p>Category: {gameInstance[0].category}</p>
                <p>Tries: {gameInstance[0].tries}</p>
                <p dangerouslySetInnerHTML={{ __html: 'Hint 1: ' + gameInstance[0].hint1 }} />
                <p dangerouslySetInnerHTML={{ __html: 'Hint 2: ' + gameInstance[0].hint2 }} />
                {gameInstance[0].options.map(option =>
                    <button key={option} onClick={async () => {
                        pickOption(option);
                    }}>{option}</button>
                )}
            </div>;
            contents = gameActiveDiv;
        }
        else {
            const gameEndDiv = <div>
                <p>Date: {gameInstance[0].date}</p>
                <p>Category: {gameInstance[0].category}</p>
                <p>Title: {gameInstance[0].title}</p>
                <p>Tries: {gameInstance[0].tries}</p>
            </div>;
            contents = gameEndDiv;
        }
    }

    return (
        <div>
            <h1 id="tabelLabel">Wikipedia daily article guessing game</h1>
            <p>Pick the title of the article from the given options. More hints are given after failed attempts.</p>
            {contents}
        </div>
    );

    async function populateGameData(category)
    {

        const response = await fetch('gameinstance?category=' + category);
        const data = await response.json();
        await setGameInstance(data);
    }
    
    async function pickOption(option)
    {
        var newState = [...gameInstance];
        newState[0].tries = newState[0].tries + 1;
        console.log(newState[0].tries);
        //todo: post message to server
        if (newState[0].title == option)
        {
            newState[0].gameEnd = true;
            //todo: send a POST request with fetch() to the server

            let formData = new FormData();
            Object.keys(newState[0]).forEach(function (key) {
                formData.append(key, newState[0][key]);
            });

            const response = await fetch("gameinstance/postgameresult", {
                method: 'POST',
                headers: {
                    'Token': "Game result"
                },
                body: formData
            });
            const result = await response.json();
            console.log(result[0]);

            
        }

        await setGameInstance(newState);
    }
}

export default App;