import * as React from "react";
import * as ReactDOM from "react-dom";
import "./style/index.scss";
import appStore from "./store";
import { Provider } from "react-redux";

// Test to see if it's working
const root = (
    <Provider store={appStore}>
        <h1>Hey</h1>
    </Provider>
);

ReactDOM.render(root, document.getElementById("root"));