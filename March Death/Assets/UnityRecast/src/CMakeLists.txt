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

add_library(Recast SHARED
  ${EPS_SRCS}
)

message(INFO "${CMAKE_SIZEOF_VOID_P}")

if (BUILD_32_BITS)
    if (APPLE)
         set(COMPILE_FLAGS "-m32 -arch i386")
         set(LINK_FLAGS "-m32 -arch i386")
    elseif (UNIX)
        set(COMPILE_FLAGS "${COMPILE_FLAGS} -m32")
        set(LINK_FLAGS "${LINK_FLAGS} -m32")
    endif()
elseif (APPLE)
    set(COMPILE_FLAGS "-m64 -arch x86_64")
    set(LINK_FLAGS "-m64 -arch x86_64")
endif()

if (APPLE OR BUILD_32_BITS)
    set_target_properties(Recast PROPERTIES COMPILE_FLAGS "${COMPILE_FLAGS}" LINK_FLAGS "${LINK_FLAGS}")
endif()

target_link_libraries(Recast
  FastLZ
)

