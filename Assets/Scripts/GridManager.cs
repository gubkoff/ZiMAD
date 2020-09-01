using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace PrototypeGame {
	public class GridManager : MonoBehaviour {
		[SerializeField] private int ColumnTileCount;
		[SerializeField] private int RowTileCount;
		[SerializeField] private List<GameObject> TilePrefabList;
		[SerializeField] private int TileForWinCount;
		[SerializeField] private int TileForChangeGravityCount;

		public static GridManager instance;
		private Tile[,] tiles;
		private Tile[,] matchedTiles;
		private bool fromTopToDownDirection = true;

		void Start() {
			fromTopToDownDirection = true;
			if (instance == null) {
				instance = this;
			} else {
				Destroy(instance);
			}

			GenerateGrid();
		}

		private void GenerateGrid() {
			tiles = new Tile[ColumnTileCount, RowTileCount];
			for (int column = 0; column < ColumnTileCount; column++) {
				for (int row = 0; row < RowTileCount; row++) {
					InstantiateTile(column, row);
				}
			}
		}

		public int GetColumnTileCount() {
			return ColumnTileCount;
		}

		public int GetRowTileCount() {
			return RowTileCount;
		}

		private void InstantiateTile(int column, int row) {
			GameObject tilePrefab = GetTilePrefab(column, row);
			if (tilePrefab != null) {
				GameObject tileInstantiate = Instantiate(tilePrefab);
				Tile tile = tileInstantiate.GetComponent<Tile>();
				tileInstantiate.name = "tile-" + column + "_" + row;
				tileInstantiate.transform.parent = gameObject.transform;
				tiles[column, row] = tile;
				tiles[column, row].column = column;
				tiles[column, row].row = row;
				tiles[column, row].UpdatePositionByGridCoordinates();
			}
		}

		private GameObject GetTilePrefab(int column, int row) {
			GameObject result = TilePrefabList[GetRandomTileIndex()];
			Tile tile = result.GetComponent<Tile>();

			if (tile != null && IsMatchedForWinByType(column, row, tile.GetTileType())) {
				result = GetTilePrefab(column, row);
			}

			return result;
		}

		private int GetRandomTileIndex() {
			int rnd = Random.Range(0, TilePrefabList.Count);
			if (rnd > TilePrefabList.Count - 1) {
				rnd = GetRandomTileIndex();
			}

			return rnd;
		}

		public void SwapTile(Tile selectedTile, Tile previousSelectedTile) {
			if (selectedTile != null && previousSelectedTile != null) {
				if (IsMatchedForWinByType(selectedTile.column, selectedTile.row, previousSelectedTile.GetTileType()) ||
				    IsMatchedForWinByType(previousSelectedTile.column, previousSelectedTile.row, selectedTile.GetTileType())) {

					tiles[selectedTile.column, selectedTile.row] = previousSelectedTile;
					tiles[previousSelectedTile.column, previousSelectedTile.row] = selectedTile;

					int tempColumn = selectedTile.column;
					int tempRow = selectedTile.row;

					selectedTile.column = previousSelectedTile.column;
					selectedTile.row = previousSelectedTile.row;

					previousSelectedTile.column = tempColumn;
					previousSelectedTile.row = tempRow;

					selectedTile.UpdatePositionByGridCoordinates();
					previousSelectedTile.UpdatePositionByGridCoordinates();

					MainGameManager.instance.AddStep();
					CheckMatch();
				}
			}
		}

		private void CheckMatch() {
			bool isMatched = false;
			matchedTiles = new Tile[ColumnTileCount, RowTileCount];
			for (int column = 0; column < ColumnTileCount; column++) {
				for (int row = 0; row < RowTileCount; row++) {
					matchedTiles[column, row] = null;
					tiles[column, row].UnSelect();
					if (IsMatchedForWin(column, row)) {
						isMatched = true;
						matchedTiles[column, row] = tiles[column, row];
					}
				}
			}

			if (isMatched) {
				StartCoroutine(MoveTiles());
			}
		}

		private IEnumerator MoveTiles() {
			DestroyMatchedTiles();
			int direction = fromTopToDownDirection ? 1 : -1;
			int startRow = fromTopToDownDirection ? 0 : RowTileCount - 1;
			for (int column = 0; column < ColumnTileCount; column++) {
				for (int row = 0; row < RowTileCount; row++) {
					if (tiles[column, row] == null) {
						yield return new WaitForSeconds(0.05f);
						int currentRow = startRow;
						int nextRow = startRow + direction;
						while (nextRow >= 0 && nextRow < RowTileCount) {
							if (tiles[column, nextRow] != null && tiles[column, currentRow] == null) {
								tiles[column, currentRow] = tiles[column, nextRow];
								tiles[column, currentRow].row = currentRow;
								tiles[column, currentRow].UpdatePositionByGridCoordinates();
								tiles[column, nextRow] = null;
							}

							currentRow += direction;
							nextRow += direction;
						}
					}
				}
			}

			FillNewTile();
		}

		private void DestroyMatchedTiles() {
			for (int column = 0; column < ColumnTileCount; column++) {
				for (int row = 0; row < RowTileCount; row++) {
					if (matchedTiles[column, row] != null) {
						MainGameManager.instance.AddScores(tiles[column, row].GetScore());
						Destroy(tiles[column, row].gameObject);
						tiles[column, row] = null;
					}
				}
			}
		}

		private void FillNewTile() {
			for (int column = 0; column < ColumnTileCount; column++) {
				for (int row = 0; row < RowTileCount; row++) {
					if (tiles[column, row] == null) {
						InstantiateTile(column, row);
					}
				}
			}
			CheckMatch();
		}

		private bool IsMatchedForWin(int column, int row) {
			return IsMatchedForWinByType(column, row, tiles[column, row].GetTileType());
		}

		private bool IsMatchedForWinByType(int column, int row, TilesTypes type) {
			return GetTilesInColumnMatchCountByType(column, row, type) >= TileForWinCount ||
			       GetTilesInRowMatchCountByType(column, row, type) >= TileForWinCount;
		}

		private int GetTilesInColumnMatchCountByType(int column, int row, TilesTypes type) {
			int result = 1;
			var rowDown = row - 1;
			var rowUp = row + 1;
			while (rowDown >= 0 && rowDown < ColumnTileCount && tiles[column, rowDown] != null &&
			       tiles[column, rowDown].GetTileType().Equals(type)) {
				result++;
				rowDown--;
			}

			while (rowUp >= 0 && rowUp < ColumnTileCount && tiles[column, rowUp] != null &&
			       tiles[column, rowUp].GetTileType().Equals(type)) {
				result++;
				rowUp++;
			}

			return result;
		}

		private int GetTilesInRowMatchCountByType(int column, int row, TilesTypes type) {
			int result = 1;
			var columnDown = column - 1;
			var columnUp = column + 1;
			while (columnDown >= 0 && columnDown < RowTileCount && tiles[columnDown, row] != null &&
			       tiles[columnDown, row].GetTileType().Equals(type)) {
				result++;
				columnDown--;
			}

			while (columnUp >= 0 && columnUp < RowTileCount && tiles[columnUp, row] != null &&
			       tiles[columnUp, row].GetTileType().Equals(type)) {
				result++;
				columnUp++;
			}

			return result;
		}
	}
}
