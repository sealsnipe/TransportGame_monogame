# Building System Testing Guide

**Date:** 2025-06-03  
**Purpose:** Test the newly implemented Multi-Tile Building System

## 🎮 **How to Test the Building System**

### **Prerequisites:**
1. Game is running (`dotnet run`)
2. Map is visible (use mouse wheel to zoom out if needed)
3. Camera controls work (WASD movement, mouse wheel zoom)

### **Building Placement Test Steps:**

#### **1. Test Farm Placement (2x2)**
1. Press **`1`** key to enter Farm placement mode
2. Move mouse around the map
3. **Expected:** Green 2x2 preview should appear on valid terrain (Grass, Farmland)
4. **Expected:** Red 2x2 preview should appear on invalid terrain (Water, Mountains)
5. Click on valid green area to place farm
6. **Expected:** Brown 2x2 building should appear with black borders

#### **2. Test Mine Placement (2x2)**
1. Press **`2`** key to enter Mine placement mode
2. Move mouse to mountain areas
3. **Expected:** Green 2x2 preview on mountains, red on other terrain
4. Click on mountain area to place mine
5. **Expected:** Gray 2x2 building should appear

#### **3. Test Steel Works Placement (3x3)**
1. Press **`3`** key to enter Steel Works placement mode
2. Move mouse around
3. **Expected:** Green 3x3 preview on valid terrain (Grass, Dirt)
4. **Expected:** Red 3x3 preview on invalid terrain (Water, Mountains, Farmland)
5. Click on valid area to place steel works
6. **Expected:** Dark gray 3x3 building should appear

#### **4. Test Food Factory Placement (3x3)**
1. Press **`4`** key to enter Food Factory placement mode
2. **Expected:** Similar behavior to Steel Works (3x3 size)
3. Click to place
4. **Expected:** Golden/yellow 3x3 building should appear

#### **5. Test Train Depot Placement (4x4)**
1. Press **`5`** key to enter Train Depot placement mode
2. **Expected:** Large 4x4 preview
3. Click on valid area to place
4. **Expected:** Blue 4x4 building should appear

#### **6. Test Station Placement (2x2)**
1. Press **`6`** key to enter Station placement mode
2. **Expected:** Brown 2x2 preview (similar to farm but different rules)
3. Click to place
4. **Expected:** Brown 2x2 building should appear

### **Advanced Features Test:**

#### **7. Test Rotation**
1. Enter any building placement mode (press 1-6)
2. Press **`R`** key multiple times
3. **Expected:** Building preview should rotate 90° each time
4. **Note:** Some buildings may look the same when rotated (rectangular)

#### **8. Test Collision Detection**
1. Place a building anywhere
2. Try to place another building overlapping the first
3. **Expected:** Red preview should appear, placement should be blocked

#### **9. Test Escape/Cancel**
1. Enter building placement mode (press 1-6)
2. Press **`ESC`** key
3. **Expected:** Placement mode should exit, no preview visible

### **Visual Verification Checklist:**

#### **Building Colors:**
- **Farm:** Brown (#8B4513)
- **Mine:** Gray (#696969) 
- **Steel Works:** Dark Gray (#2F4F4F)
- **Food Factory:** Gold (#DAA520)
- **Train Depot:** Blue (#4682B4)
- **Station:** Brown (#8B4513)

#### **Building Sizes:**
- **2x2:** Farm, Mine, Station
- **3x3:** Steel Works, Food Factory  
- **4x4:** Train Depot

#### **Visual Elements:**
- ✅ Black borders around placed buildings
- ✅ Small center markers on buildings
- ✅ Green preview for valid placement
- ✅ Red preview for invalid placement
- ✅ Semi-transparent preview effect

### **Expected Log Output:**

When placing buildings, you should see console output like:
```
[23:XX:XX.XXX] [INFO] Started building placement for: farm
[EVENT] Building placed: farm at (X, Y)
[23:XX:XX.XXX] [INFO] Placed building farm at (X,Y)
```

### **Troubleshooting:**

#### **If buildings don't appear:**
1. Check zoom level (zoom out with mouse wheel)
2. Check if you're in the right area of the map
3. Look for console error messages

#### **If placement doesn't work:**
1. Verify you pressed the correct number key (1-6)
2. Check if you're clicking on valid terrain
3. Make sure you're not overlapping existing buildings

#### **If preview doesn't show:**
1. Move mouse around more
2. Check if you're in placement mode (press 1-6 again)
3. Verify the building system loaded correctly in console

### **Success Criteria:**

✅ **All 6 building types can be placed**  
✅ **Buildings appear with correct colors and sizes**  
✅ **Collision detection prevents overlapping**  
✅ **Terrain validation works correctly**  
✅ **Rotation works (press R)**  
✅ **Cancel works (press ESC)**  
✅ **No crashes or errors**

### **Screenshot Locations:**

Take screenshots showing:
1. **Building placement preview** (green/red indicators)
2. **Successfully placed buildings** of different types
3. **Multiple buildings** on the map
4. **Collision detection** (red preview when overlapping)

Press **F9** to take screenshots during testing.

---

**Note:** This is the foundation of the building system. Future phases will add:
- Building selection UI/menu
- Resource production and consumption
- Construction animations
- Building upgrades and management
