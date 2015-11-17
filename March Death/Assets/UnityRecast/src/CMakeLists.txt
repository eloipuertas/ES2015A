# Based on TrinityCore CMake files
#
# This file is free software; as a special exception the author gives
# unlimited permission to copy and/or distribute it, with or without
# modifications, as long as this notice is preserved.
#
# This program is distributed in the hope that it will be useful, but
# WITHOUT ANY WARRANTY, to the extent permitted by law; without even the
# implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

file(GLOB_RECURSE sources_Unity				Unity/*.cpp						Unity/*.h)
file(GLOB_RECURSE sources_Recast			Recast/Source/*.cpp				Recast/Include/*.h)
file(GLOB_RECURSE sources_Detour			Detour/Source/*.cpp				Detour/Include/*.h)
file(GLOB_RECURSE sources_DetourCrowd		DetourCrowd/Source/*.cpp		DetourCrowd/Include/*.h)
file(GLOB_RECURSE sources_DetourTileCache   DetourTileCache/Source/*.cpp	DetourTileCache/Include/*.h)

set(EPS_SRCS
  ${EPS_SRCS}
  ${sources_Unity}
  ${sources_Recast}
  ${sources_Detour}
  ${sources_DetourCrowd}
  ${sources_DetourTileCache}
)

include_directories(
  ${CMAKE_BINARY_DIR}
  ${CMAKE_CURRENT_BINARY_DIR}
  ${CMAKE_CURRENT_SOURCE_DIR}/Unity
  ${CMAKE_CURRENT_SOURCE_DIR}/Recast/Include
  ${CMAKE_CURRENT_SOURCE_DIR}/Recast/Source
  ${CMAKE_CURRENT_SOURCE_DIR}/Detour/Include
  ${CMAKE_CURRENT_SOURCE_DIR}/Detour/Source
  ${CMAKE_CURRENT_SOURCE_DIR}/DetourCrowd/Include
  ${CMAKE_CURRENT_SOURCE_DIR}/DetourCrowd/Source
  ${CMAKE_CURRENT_SOURCE_DIR}/DetourTileCache/Include
  ${CMAKE_CURRENT_SOURCE_DIR}/DetourTileCache/Source

  ${CMAKE_SOURCE_DIR}/dep/fastlz
)

if (APPLE)
    set(MACOSX_BUNDLE_BUNDLE_NAME Recast)
    add_library(Recast SHARED ${EPS_SRCS})
    set_target_properties(Recast PROPERTIES MACOSX_BUNDLE TRUE)
else()
    add_library(Recast SHARED ${EPS_SRCS})
endif()

if (BUILD_32_BITS AND NOT WIN)
    set(COMPILE_FLAGS "${COMPILE_FLAGS} -m32")
    set(LINK_FLAGS "${LINK_FLAGS} -m32")
elseif (NOT WIN)
    set(COMPILE_FLAGS "${COMPILE_FLAGS} -m64")
    set(LINK_FLAGS "${LINK_FLAGS} -m64")
endif()

if (NOT WIN OR BUILD_32_BITS)
    set_target_properties(Recast PROPERTIES COMPILE_FLAGS "${COMPILE_FLAGS}" LINK_FLAGS "${LINK_FLAGS}")
endif()

target_link_libraries(Recast
  FastLZ
)
