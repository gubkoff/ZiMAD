using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PrototypeGame {
	[RequireComponent(typeof(BoxCollider2D))]
	public class Tile : MonoBehaviour {
		private bool isSelected = false;
		private Vector2 currentPosition;
		private SpriteRenderer renderer;
		private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
		private static Tile previousSelectedTile;
		private static bool isChangeGravity;
		private Vector2 size;
		public int row;
		public int column;

		[SerializeField] private TilesTypes type;
		[SerializeField] private int score;


		private void Awake() {
			previousSelectedTile = null;
			renderer = gameObject.GetComponent<SpriteRenderer>();
			size = gameObject.GetComponent<BoxCollider2D>().size;
			UpdateCurrentPosition();
		}

		public void UpdatePositionByGridCoordinates() {
			transform.position = new Vector2(GetColumnPosition(), GetRowPosition());
			UpdateCurrentPosition();
		}

		private float GetColumnPosition() {
			return column * size.y - (GridManager.instance.GetColumnTileCount() / 2 * size.y - size.y / 2);
		}

		private float GetRowPosition() {
			return row * size.x - (GridManager.instance.GetRowTileCount() / 2 * size.x - size.x / 2);
		}

		public void UpdateCurrentPosition() {
			currentPosition = transform.position;
		}

		public Vector2 GetCurrentPosition() {
			return currentPosition;
		}

		public void UpdatePosition(Vector2 newPosition) {
			transform.position = newPosition;
		}

		private void OnMouseDown() {
			if (isSelected) {
				UnSelect();
			} else {
				if (previousSelectedTile == null) {
					Select();
				} else {
					if (previousSelectedTile != this && IsAdjacent()) {
						GridManager.instance.SwapTile(this, previousSelectedTile);
					}
					UnSelect();
				}
			}
		}

		private bool IsAdjacent() {
			bool result = false;
			if (previousSelectedTile != null) {
				if (Mathf.Abs(previousSelectedTile.column - column) == 1 || Mathf.Abs(previousSelectedTile.row - row) == 1) {
					result = true;
				}
			} else {
				result = false;
			}

			return result;
		}

		private void Select() {
			previousSelectedTile = this;
			isSelected = true;
			renderer.color = selectedColor;
		}

		public void UnSelect() {
			previousSelectedTile = null;
			isSelected = false;
			renderer.color = Color.white;
		}

		public TilesTypes GetTileType() {
			return type;
		}

		public int GetScore() {
			return score;
		}
	}
}
