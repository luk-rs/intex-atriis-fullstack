import React from 'react';
import logo from './logo.svg';
import './App.css';
import BestBuyProducts from "./BestBuy/BestBuyProducts";

function App() {
  return (
    <div className="App">
      <img src={logo} height={100} width={100} className="App-logo" alt="logo" />
      <BestBuyProducts/>
    </div>
  );
}

export default App;
