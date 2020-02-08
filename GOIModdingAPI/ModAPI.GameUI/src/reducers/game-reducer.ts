import { createReducer, PayloadAction } from "@reduxjs/toolkit";
import { setPaused, setScene } from "../actions/game-actions";
import SceneType from "../data/game/scene-type";

type GameState = {
    scene: SceneType,
    paused: boolean
};

const getDefaultState = (): GameState => ({
    scene: SceneType.Menu,
    paused: false
});

export default createReducer<GameState>(getDefaultState(), {
    [setPaused.type]: (state, action: PayloadAction<boolean>) => {
        state.paused = action.payload;
    },
    [setScene.type]: (state, action: PayloadAction<SceneType>) => {
        state.scene = action.payload;
    }
});