import { createAction } from "@reduxjs/toolkit";
import SceneType from "../data/game/scene-type";

export const setPaused = createAction<boolean>("SET_GAME_PAUSED");
export const setScene = createAction<SceneType>("SET_GAME_SCENE");