# Based on TrinityCore CMake files
#
# This file is free software; as a special exception the author gives
# unlimited permission to copy and/or distribute it, with or without
# modifications, as long as this notice is preserved.
#
# This program is distributed in the hope that it will be useful, but
# WITHOUT ANY WARRANTY, to the extent permitted by law; without even the
# implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
project(FastLZ)

file(GLOB_RECURSE sources_All *.c *.h)

set(EPS_SRCS
  ${EPS_SRCS}
  ${sources_All}
)

include_directories(
  ${CMAKE_BINARY_DIR}
  ${CMAKE_CURRENT_BINARY_DIR}
)

add_library(FastLZ
  ${EPS_SRCS}
)

if(BUILD_32_BITS)
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

if (UNIX OR APPLE)
    set(COMPILE_FLAGS "${COMPILE_FLAGS} -fPIC")
endif()

if (UNIX OR APPLE OR BUILD_32_BITS)
    set_target_properties(FastLZ PROPERTIES COMPILE_FLAGS "${COMPILE_FLAGS}" LINK_FLAGS "${LINK_FLAGS}")
endif()
