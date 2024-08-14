import { useState } from 'react';
import './App.css';

const categories = ["Physics", "Countries in Europe", "States of the United States", "Chemical elements", "Fish common names", "Chinese inventions"];

function App() {
    const [gameInstance, setGameInstance] = useState();

    var contents = <p><em>Loading... </em></p>;
    if (gameInstance === undefined)
    {
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
    else if (gameInstance != "Loading")
    {
        if (!gameInstance[0].gameEnd) {
            const gameActiveDiv = <div>
                <p>Date: {gameInstance[0].date}</p>
                <p>Category: {gameInstance[0].category}</p>
                <p>Tries: {gameInstance[0].tries}</p>
                <p dangerouslySetInnerHTML={{ __html: 'Hint: ' + gameInstance[0].hint }} />
                {gameInstance[0].options.map(option =>
                    <button key={option} onClick={async () => {
                        pickOption(option);
                    }}>{option}</button>
                )}
            </div>;
            contents = gameActiveDiv;
        }
        else
        {
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
        //Initially set the state to something that allows to frontend to know it's being updated
        await setGameInstance("Loading");

        const response = await fetch('gameinstance?category=' + category);
        const data = await response.json();
        data[0].hint = data[0].hints[0];
        await setGameInstance(data);
    }
    
    async function pickOption(option)
    {
        var newState = [...gameInstance];
        newState[0].tries = newState[0].tries + 1;
        if (newState[0].tries < newState[0].hints.length)
        {
            newState[0].hint += newState[0].hints[newState[0].tries];
        }
        if (newState[0].title == option)
        {
            newState[0].gameEnd = true;

            let formData = new FormData();
            Object.keys(newState[0]).forEach(function (key) {
                formData.append(key, newState[0][key]);
            });

            fetch("gameinstance/postgameresult", {
                method: 'POST',
                headers: {
                    'Token': "Game result"
                },
                body: formData
            });
            /* If you await the fetch and store the response to variable named "response", use this to see the response
            const result = response.json();
            console.log(result);
            */
        }

        await setGameInstance(newState);
    }
}

export default App;